using EnananBot.Cache;
using EnananBot.Modules.Base;
using EnananBot.Services;
using EnananBot.Utils;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace EnananBot.Modules.Commands;

/// <summary>
/// The main command module for managing personal roles.
/// Handles creation, editing, and deletion with strict validation and database synchronization.
/// </summary>
[SlashCommand("role", "Commands for creating, editing, and deleting personal roles.")]
public class RoleCommands(
    GuildCache guildCache,
    ValidationService validationService,
    MessageService messages)
    : RegisteredGuildModule(validationService) // Inherits the "EnsureGuildRegistered" check
{
    private readonly ValidationService _validationService = validationService;

    /// <summary>
    /// Command: /role create
    /// The complex flow:
    /// 1. Validate inputs (Name length, Color valid, User doesn't have a role yet).
    /// 2. Create Role in Discord.
    /// 3. Assign Role to User in Discord.
    /// 4. Save Role ID to Database.
    /// 5. If Database save fails, DELETE the Discord role (Rollback) to maintain consistency.
    /// </summary>
    [SubSlashCommand(
        "create",
        "This command is for making you a new role and adding it to the list!")]
    public async Task CreateRole(
        [SlashCommandParameter(Name = "title", Description = "The name for your role!", MinLength = 3, MaxLength = 32)]
        string name,
        [SlashCommandParameter(Name = "color", Description = "Can be a hex code or a color name!", MinLength = 3)]
        string colorString,
        [SlashCommandParameter(
            Name = "decoration", Description = "[OPTIONAL] A bunch of fancy icons if you want to add flare to your role!",
            AutocompleteProviderType = typeof(AutocompleteService.DecoratorsProvider))] // Uses the Autocomplete Service we defined earlier
        string? decoration = null)
    {
        // 1. Pre-Check: Is the server set up?
        if (!await EnsureGuildRegisteredAsync())
            return;

        await ResponseUtils.DeferAsync(Context);

        var user = (GuildUser)Context.User;
        var username = user.Nickname ?? user.GlobalName ?? user.Username;
        
        try
        {
            var guild = Context.Guild!;

            // 2. Input Sanitization
            var roleName = name.Trim().NormalizeSpaces();
            var roleColor = colorString.ToLowerInvariant().StripInvalidChars();

            // 3. Validation Chain
            // Check 250 role limit
            if (!await TryValidateAsync(
                    _validationService.GuildHasMaxRoleCount(guild.Roles.Count, username)))
                return;

            // Check if the user already has a role
            if (!await TryValidateAsync(
                    await _validationService.UserAlreadyHasUniqueRoleAsync(
                        guild.Id, user.Id, username)))
                return;

            // Check name rules (regex)
            if (!await TryValidateAsync(
                    _validationService.ValidateRoleName(roleName, username)))
                return;

            // Check color rules
            if (!await TryValidateAsync(
                    _validationService.ValidateColor(roleColor, username)))
                return;

            // Apply decorations (e.g. "★ Role Name ★")
            if (!string.IsNullOrWhiteSpace(decoration))
                roleName = roleName.DecorateRoleName(decoration);

            // 4. Create Role on Discord
            var role = await guild.CreateRoleAsync(
                new RoleProperties()
                    .WithName(roleName)
                    .WithColor(ColorUtils.GetDiscordColor(roleColor))
                    .WithMentionable(false)); // Personal roles shouldn't be mentionable by default

            // 5. Assign role to User
            await guild.AddUserRoleAsync(user.Id, role.Id);
            
            try
            {
                // 6.1. Get the bot's member object in this guild
                var botMember = await guild.GetUserAsync(1460482352001323185);

                // 6.2. Get the highest role the bot currently has
                var botTopRole = botMember.GetRoles(guild).MaxBy(r => r.Position);

                // 6.3. Calculate a safe target position:
                var targetPosition = botTopRole.Position - 1;

                // 6.4. Clamp the value so it never goes below @everyone
                targetPosition = Math.Max(targetPosition, 1);

                // 6.5. Create the role position change object
                var pos = new RolePositionProperties(role.Id).WithPosition(targetPosition);

                // 6.6. Apply the role position change
                await guild.ModifyRolePositionsAsync([pos]);
            }
            catch
            {
                // Intentionally ignore any failures here
            }
            
            // 7. Save to Database
            if (await guildCache.SetRoleAsync(guild.Id, user.Id, role.Id))
            {
                // Success
                await ResponseUtils.SendSimpleResponse(
                    Context,
                    messages.RoleCreateSuccess(username));
            }
            else
            {
                // CRITICAL FAILURE: DB Write failed
                // Rollback: Delete the role we just created so the user isn't stuck with a "ghost" role
                await role.DeleteAsync();

                await ResponseUtils.SendSimpleResponse(
                    Context,
                    messages.SystemError(),
                    true);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            await ResponseUtils.SendSimpleResponse(
                Context,
                messages.RoleCreateError(username),
                true);
        }
        finally
        {
            ResponseUtils.Clear(Context);
        }
    }

    /// <summary>
    /// Command: /role edit
    /// Updates an existing role.
    /// </summary>
    [SubSlashCommand(
        "edit",
        "Need to do some touch ups? This command will help with the role you've already made.")]
    public async Task EditRole(
        [SlashCommandParameter(Name = "title", Description = "[OPTIONAL] The name for your role!", MinLength = 3, MaxLength = 32)]
        string? newName = null,
        [SlashCommandParameter(Name = "color", Description = "[OPTIONAL] Can be a hex code or a color name!", MinLength = 3)]
        string? newColorString = null,
        [SlashCommandParameter(
            Name = "decoration", Description = "[OPTIONAL] A bunch of fancy icons if you want to add flare to your role!",
            AutocompleteProviderType = typeof(AutocompleteService.DecoratorsProvider))]
        string? decoration = null)
    {
        if (!await EnsureGuildRegisteredAsync())
            return;

        await ResponseUtils.DeferAsync(Context);
        
        var user = (GuildUser)Context.User;
        var username = user.Nickname ?? user.GlobalName ?? user.Username;

        try
        {
            var guild = Context.Guild!;

            // Check if they actually have a role to edit
            if (!await TryValidateAsync(
                    await _validationService.UserDoesNotHaveUniqueRoleAsync(
                        guild.Id, user.Id, username)))
                return;
            
            // Validate inputs
            if (!await TryValidateAsync(
                    _validationService.ValidateIfAllEmpty(newName, newColorString, decoration, username)))
                return;
            
            if (!await TryValidateAsync(
                    _validationService.ValidateRoleName(newName, username, false)))
                return;
            
            if (!await TryValidateAsync(
                    _validationService.ValidateColor(newColorString, username, false)))
                return;
            
            // Fetch Role object from Discord
            var roleId = await guildCache.GetRoleAsync(guild.Id, user.Id);
            var role = await guild.GetRoleAsync(roleId!.Value);
            
            // Build new values by reusing existing ones
            var finalName = !string.IsNullOrWhiteSpace(newName)
                ? newName.Trim().NormalizeSpaces()
                : role.Name;
            var finalColor = role.Color;

            // Apply decoration if provided
            if (!string.IsNullOrWhiteSpace(decoration))
            {
                finalName = finalName.DecorateRoleName(decoration);
            }

            // Apply new color if provided
            if (!string.IsNullOrWhiteSpace(newColorString))
            {
                var normalizedColor =
                    newColorString.ToLowerInvariant().StripInvalidChars();

                finalColor = ColorUtils.GetDiscordColor(normalizedColor);
            }
            
            // If nothing actually changed, bail early
            if (finalName == role.Name && finalColor == role.Color)
            {
                await ResponseUtils.SendSimpleResponse(
                    Context,
                    messages.RoleEditNoChanges(username),
                    true);
                return;
            }
            
            // Update Discord
            await role.ModifyAsync(r =>
            {
                r.Name = finalName;
                r.Color = finalColor;
            });
            
            await ResponseUtils.SendSimpleResponse(
                Context,
                messages.RoleEditSuccess(username));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            await ResponseUtils.SendSimpleResponse(
                Context,
                messages.RoleEditError(username),
                true);
        }
        finally
        {
            ResponseUtils.Clear(Context);
        }
    }

    /// <summary>
    /// Command: /role delete
    /// Removes the user's personal role from Discord and the Database.
    /// </summary>
    [SubSlashCommand(
        "delete",
        "Not feeling that one? That's alright, this command will take your role off the list.")]
    public async Task DeleteRole()
    {
        if (!await EnsureGuildRegisteredAsync())
            return;

        await ResponseUtils.DeferAsync(Context);
        
        var user = (GuildUser)Context.User;
        var username = user.Nickname ?? user.GlobalName ?? user.Username;

        try
        {
            var guild = Context.Guild!;

            // Verify they have a role
            if (!await TryValidateAsync(
                    await _validationService.UserDoesNotHaveUniqueRoleAsync(
                        guild.Id, user.Id, username)))
                return;

            // Get ID
            var roleId = await guildCache.GetRoleAsync(guild.Id, user.Id);
            if (roleId == null)
            {
                await ResponseUtils.SendSimpleResponse(
                    Context,
                    messages.RoleDeleteError(username),
                    true);
                return;
            }

            // Delete from Discord
            var role = await guild.GetRoleAsync(roleId.Value);
            await role.DeleteAsync();

            // Clear from Database
            if (await guildCache.ClearRoleAsync(guild.Id, user.Id))
            {
                await ResponseUtils.SendSimpleResponse(
                    Context,
                    messages.RoleDeleteSuccess(username));
            }
            else
            {
                // This implies the role was deleted from Discord, but the DB failed to update.
                // This creates a minor desync, but since the role is gone, it's not a critical "Ghost Role" issue.
                Console.WriteLine(
                    $"[CRITICAL] Data desync for {guild.Id}:{user.Id}");

                await ResponseUtils.SendSimpleResponse(
                    Context,
                    messages.SystemError(),
                    true);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            await ResponseUtils.SendSimpleResponse(
                Context,
                messages.RoleDeleteError(username),
                true);
        }
        finally
        {
            ResponseUtils.Clear(Context);
        }
    }
}