using EnananBot.Cache;
using EnananBot.Utils;

namespace EnananBot.Services;

/// <summary>
/// A simple "Result" pattern implementation used to pass validation status back to commands.
/// If IsSuccess is false, ErrorMessage contains the user-facing reason.
/// </summary>
public record Validation(bool IsSuccess, string ErrorMessage)
{
    public static Validation Success() =>
        new(true, string.Empty);

    public static Validation Failure(string message) =>
        new(false, message);
}

/// <summary>
/// Centralized service for validating user input and guild state before executing commands.
/// Returns a Validation record containing the success state and a pre-formatted error message if failed.
/// </summary>
/// <param name="cache">Used to check database state (guild registration, user roles).</param>
/// <param name="messages">Used to fetch localized/formatted error messages.</param>
public sealed class ValidationService(
    GuildCache cache,
    MessageService messages)
{
    /// <summary>
    /// Checks if the guild is currently registered in the bot's database.
    /// Commands that require database storage (like custom roles) should check this first.
    /// </summary>
    public async Task<Validation> GuildIsRegisteredAsync(ulong guildId)
    {
        var isRegistered =
            await cache.IsGuildRegisteredAsync(guildId);

        if (!isRegistered)
            return Validation.Failure(
                messages.ServerIsNotRegistered());

        return Validation.Success();
    }

    /// <summary>
    /// Checks if the guild has hit the Discord hard limit of 250 roles.
    /// Prevents the bot from attempting to create a role that API would reject.
    /// </summary>
    public Validation GuildHasMaxRoleCount(int roleCount, string username)
    {
        return roleCount >= 250
            ? Validation.Failure(
                messages.ValidationRoleAmountIsAtMax(username))
            : Validation.Success();
    }

    /// <summary>
    /// Validates that a role name is not null or whitespace.
    /// </summary>
    public Validation ValidateRoleName(string? roleName, string username)
    {
        return string.IsNullOrWhiteSpace(roleName)
            ? Validation.Failure(
                messages.ValidationEmptyName(username))
            : Validation.Success();
    }

    /// <summary>
    /// Validates that a color string is BOTH present AND a valid hex code.
    /// Used for commands where color is a mandatory argument.
    /// </summary>
    public Validation ValidateRequiredColor(string? colorString, string username)
    {
        // 1. Check existence
        if (string.IsNullOrWhiteSpace(colorString))
            return Validation.Failure(
                messages.ValidationEmptyColor(username));

        // 2. Check validity (Hex format)
        return ColorUtils.NormalizeColorString(colorString) != null
            ? Validation.Success()
            : Validation.Failure(
                messages.ValidationInvalidColor(username));
    }
    
    /// <summary>
    /// Validates a color string ONLY if it is present. 
    /// If empty/null, it returns Success (allowing the command to proceed without a color change).
    /// Used for "Edit" commands where parameters are optional.
    /// </summary>
    public Validation ValidateOptionalColor(string? colorString, string username)
    {
        if (string.IsNullOrWhiteSpace(colorString))
            return Validation.Success();

        return ColorUtils.NormalizeColorString(colorString) != null
            ? Validation.Success()
            : Validation.Failure(
                messages.ValidationInvalidColor(username));
    }

    /// <summary>
    /// Ensures the user did not submit an edit command with no changes.
    /// Fails validation if role name, color, and decorator are all null or whitespace.
    /// </summary>
    public Validation ValidateIfAllEmpty(
        string? newRoleName, string? colorString, string? decorator, string username)
    {
        if (string.IsNullOrWhiteSpace(newRoleName) &&
            string.IsNullOrWhiteSpace(colorString) &&
            string.IsNullOrWhiteSpace(decorator))
            return Validation.Failure(messages.ValidationAllInputsAreEmpty(username));

        return Validation.Success();
    }


    /// <summary>
    /// Checks if the user already has a custom "Unique Role" assigned in the database.
    /// Used to prevent users from creating multiple personal roles.
    /// </summary>
    public async Task<Validation> UserAlreadyHasUniqueRoleAsync(
        ulong guildId, ulong userId, string username)
    {
        var roleId =
            await cache.GetRoleAsync(guildId, userId);

        return roleId != null
            ? Validation.Failure(
                messages.ValidationUserHasRole(username))
            : Validation.Success();
    }

    /// <summary>
    /// Checks if the user DOES NOT have a custom role.
    /// Used for commands that require an existing role (Edit, Delete, Info).
    /// </summary>
    public async Task<Validation> UserDoesNotHaveUniqueRoleAsync(
        ulong guildId, ulong userId, string username)
    {
        var roleId =
            await cache.GetRoleAsync(guildId, userId);

        return roleId == null
            ? Validation.Failure(
                messages.ValidationUserDoesNotHaveRole(username))
            : Validation.Success();
    }
}