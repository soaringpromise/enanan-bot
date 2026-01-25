using EnananBot.Services;
using EnananBot.Utils;
using NetCord.Services.ApplicationCommands;

// ReSharper disable NullableWarningSuppressionIsUsed => Commands will always be ran in a server context (Guild != null)

namespace EnananBot.Modules.Base;

/// <summary>
/// The base class for all bot command modules.
/// Provides shared helper methods for validation and error handling.
/// </summary>
public abstract class RegisteredGuildModule(ValidationService validationService)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    /// <summary>
    /// Checks if the current guild exists in the database.
    /// If not, it automatically sends an error response to the user.
    /// </summary>
    /// <returns>True if the guild is valid and registered; otherwise, False.</returns>
    protected async Task<bool> EnsureGuildRegisteredAsync()
    {
        // Ask the ValidationService if this guild ID is in the cache.
        var result = await validationService.GuildIsRegisteredAsync(Context.Guild!.Id);

        if (result.IsSuccess)
            return true;

        // If validation fails, handle the UI here so the command doesn't have to.
        await ResponseUtils.SendSimpleResponse(Context, result.ErrorMessage, true);
        return false;
    }
    
    /// <summary>
    /// A generic helper to process any Validation result.
    /// If the result is a Failure, it sends the error message to the user automatically.
    /// </summary>
    /// <param name="result">The result object from a Service method.</param>
    /// <returns>True if the operation should proceed; False if it failed.</returns>
    protected async Task<bool> TryValidateAsync(Validation result)
    {
        if (result.IsSuccess)
            return true;

        // Auto-reply with the specific error (e.g., "Role Name too long", "Invalid Hex Code").
        await ResponseUtils.SendSimpleResponse(Context, result.ErrorMessage, true);
        return false;
    }
}