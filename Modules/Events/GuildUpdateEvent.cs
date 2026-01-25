using NetCord.Gateway;
using NetCord.Hosting.Gateway;

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace EnananBot.Modules.Events;

/// <summary>
/// Event handler for Server Updates.
/// Serves as a security guard to enforce a maximum member count policy.
/// </summary>
public class GuildUpdateEvent : IGuildUpdateGatewayHandler
{
    // The hard cap on server size
    // Since this is a private/personal server bot, we don't want it in massive servers
    // where it could be abused or hit API rate limits
    private const int MaxMemberCount = 50;

    /// <summary>
    /// Checks the server status whenever it updates (e.g., name change, icon change, member count update)
    /// </summary>
    public async ValueTask HandleAsync(Guild arg)
    {
        // If the server is small enough, do nothing
        if (arg.UserCount <= MaxMemberCount) return;

        // If we are here, the server is too big
        Console.WriteLine($"[Security] Leaving Guild '{arg.Name}' (ID: {arg.Id}). Member count {arg.UserCount} exceeds limit {MaxMemberCount}.");

        try
        {
            // Auto-leave the server to protect the bot's resources
            await arg.LeaveAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Error] Failed to leave guild: {e.Message}");
        }
    }
}