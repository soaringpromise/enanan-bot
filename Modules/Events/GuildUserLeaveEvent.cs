using System.Collections.Concurrent;
using EnananBot.Cache;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace EnananBot.Modules.Events;

/// <summary>
/// Event handler triggered when a user leaves (or is kicked/banned from) a server.
/// Responsible for cleaning up the user's data from the internal cache/database.
/// </summary>
public class GuildUserLeaveEvent(GuildCache guildCache) : IGuildUserRemoveGatewayHandler
{
    // A dictionary of locks, keyed by Guild ID
    // This prevents race conditions if multiple users leave the SAME server simultaneously
    private static readonly ConcurrentDictionary<ulong, SemaphoreSlim> GuildLocks = new();

    public async ValueTask HandleAsync(GuildUserRemoveEventArgs arg)
    {
        // Ignore bots
        if (arg.User.IsBot) return;

        // Get the lock for this specific server, if it doesn't exist, create a new one
        var gate = GuildLocks.GetOrAdd(arg.GuildId, _ => new SemaphoreSlim(1, 1));
        
        // Wait until it is safe to enter (no other thread is modifying this server's cache)
        await gate.WaitAsync();

        try
        {
            // Safely remove the user from the persistent database/cache
            await guildCache.RemoveUserAsync(arg.GuildId, arg.User.Id);
            Console.WriteLine($"[Cache] User removed: {arg.User.Id} from {arg.GuildId}");
        }
        catch (Exception e)
        {
            // Log errors but don't crash; a failure here just means we have a bit of stale data, not a critical failure
            Console.WriteLine($"[Error] Failed to remove user {arg.User.Id} from {arg.GuildId}: {e}");
        }
        finally
        {
            // ALWAYS release the lock
            gate.Release();
        }
    }
}