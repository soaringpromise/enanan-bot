using System.Text;
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
/// Slash Command Module for Server Administration.
/// Base Command: /enanan
/// locked to users with 'Administrator' permission by default.
/// </summary>
[SlashCommand(
    "enanan",
    "Bot setup and administration commands.",
    DefaultGuildPermissions = Permissions.Administrator)]
public class ManagementCommands(
    GuildCache guildCache,
    ValidationService validationService,
    MessageService messages)
    : RegisteredGuildModule(validationService)
{
    /// <summary>
    /// Command: /enanan setup
    /// Manually registers the current guild in the database and bulk-imports all current members.
    /// This should not be run unless the bot did not automatically register the guild.
    /// </summary>
    [SubSlashCommand("setup", "Manual setup if the bot doesn't automatically register.")]
    public async Task InitialSetup()
    {
        await ResponseUtils.DeferAsync(Context);

        var user = (GuildUser)Context.User;
        var username = user.Nickname ?? user.GlobalName ?? user.Username;
        
        try
        {
            var guild = Context.Guild!;

            // 1. Prevent duplicate setup
            if (await guildCache.IsGuildRegisteredAsync(guild.Id))
            {
                await ResponseUtils.SendSimpleResponse(
                    Context,
                    messages.ServerIsAlreadyRegistered(),
                    true);
                return;
            }

            // 2. Gather all human users (Bots don't get custom roles)
            var userIds = guild.Users.Values
                .Where(u => !u.IsBot)
                .Select(u => u.Id)
                .ToArray();

            if (userIds.Length == 0)
            {
                await ResponseUtils.SendSimpleResponse(
                    Context,
                    messages.NoUsersToRegister(),
                    true);
                return;
            }

            // 3. Bulk insert into database
            await guildCache.AddUsersAsync(guild.Id, userIds);

            await ResponseUtils.SendSimpleResponse(
                Context,
                messages.ServerSetupSuccess(username),
                true);
        }
        catch (Exception e)
        {
            Console.WriteLine("There was an issue setting up a server: " + e.Message);

            await ResponseUtils.SendSimpleResponse(
                Context,
                messages.ServerSetupError(username),
                true);
        }
        finally
        {
            ResponseUtils.Clear(Context);
        }
    }

    /// <summary>
    /// Command: /enanan list
    /// Generates a report of every user and their custom role.
    /// Sent to DMs to avoid cluttering the chat.
    /// </summary>
    [SubSlashCommand("list", "Lists all registered users (sent via DMs).")]
    public async Task ListAllUsers()
    {
        if (!await EnsureGuildRegisteredAsync())
            return;

        await ResponseUtils.DeferAsync(Context);

        try
        {
            var guild = Context.Guild!;
            var sb = new StringBuilder();

            sb.AppendLine(messages.UserListHeader(guild.Name));
            sb.AppendLine();

            // Iterate over all human users
            foreach (var guildUser in guild.Users.Values.Where(u => !u.IsBot))
            {
                // Retrieve their custom role ID from the DB
                var roleId = await guildCache.GetRoleAsync(guild.Id, guildUser.Id);

                // Resolve the ID to a Role Name (if the role still exists in Discord)
                var roleName =
                    roleId.HasValue &&
                    guild.Roles.TryGetValue(roleId.Value, out var role)
                        ? role.Name
                        : "—";

                var displayName =
                    $"{guildUser.Username} (Nick: {guildUser.Nickname ?? "—"})";

                sb.AppendLine($"- {displayName} | Role: {roleName}");
            }

            var dmChannel = await Context.User.GetDMChannelAsync();

            // Smart Reporting:
            // Discord has a 2000-character limit per message.
            // If the report is too long (safe limit 1900), save it as a text file instead.
            if (sb.Length > 1900)
            {
                using var stream =
                    new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));

                await dmChannel.SendMessageAsync(
                    new MessageProperties().WithAttachments(
                        [new AttachmentProperties("users.txt", stream)]));
            }
            else
            {
                await dmChannel.SendMessageAsync(
                    new MessageProperties().WithContent(sb.ToString()));
            }

            // Confirm to the admin in the server that the DM was sent.
            await ResponseUtils.SendSimpleResponse(
                Context,
                messages.MessageSentToDMs(),
                true);
        }
        catch (RestException e)
            when (e.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            // Handle cases where the Admin has DMs blocked.
            await ResponseUtils.SendSimpleResponse(
                Context,
                messages.AdminDMsAreClosed(),
                true);
        }
        finally
        {
            ResponseUtils.Clear(Context);
        }
    }

    /// <summary>
    /// Command: /enanan welcome #channel
    /// Configures the channel where the "GuildUserJoinEvent" will post welcome images.
    /// </summary>
    [SubSlashCommand("welcome", "Sets or updates the welcome channel.")]
    public async Task SetWelcomeChannel(TextGuildChannel channel)
    {
        if (!await EnsureGuildRegisteredAsync())
            return;

        await ResponseUtils.DeferAsync(Context);

        try
        {
            var guildId = Context.Guild!.Id;

            await guildCache.SetWelcomeChannelAsync(guildId, channel.Id);

            await ResponseUtils.SendSimpleResponse(
                Context,
                messages.WelcomeChannelSet(channel.Id),
                true);
        }
        catch (Exception e)
        {
            Console.WriteLine($"There was an issue setting up a welcome channel in {channel.GuildId}: {e.Message}");
        }
        finally
        {
            ResponseUtils.Clear(Context);
        }
    }
}