using System.Collections.Concurrent;
using EnananBot.Cache;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace EnananBot.Modules.Events;

/// <summary>
/// Event handler triggered when the bot joins a new server OR when the bot connects and loads existing servers.
/// Responsible for initial security checks and bulk-loading user data.
/// </summary>
public class GuildJoinEvent(GuildCache guildCache) : IGuildCreateGatewayHandler
{
    // Concurrency lock to ensure we don't try to initialize the same server twice simultaneously
    private static readonly ConcurrentDictionary<ulong, SemaphoreSlim> GuildLocks = new();

    // The hard cap on server size
    private const int MaxMemberCount = 50;

    public async ValueTask HandleAsync(GuildCreateEventArgs arg)
    {
        // Get the lock for this specific server
        var gate = GuildLocks.GetOrAdd(arg.Guild!.Id, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();

        try
        {
            // If the server exceeds the bot limit, leave immediately
            // This protects the bot from being added to massive servers
            if (arg.Guild.UserCount > MaxMemberCount)
            {
                Console.WriteLine($"[Security] Leaving {arg.Guild.Name} ({arg.Guild.UserCount} members).");
                await arg.Guild.LeaveAsync();
                return;
            }
            
            // Extract all User IDs currently in the server
            var userIds = arg.Guild.Users.Values.Select(u => u.Id).ToArray();
            
            // Bulk-register them in the database
            // This ensures that existing members can use commands immediately without
            // needing to leave and rejoin the server to trigger a registration event
            await guildCache.AddUsersAsync(arg.Guild.Id, userIds);
            
            Console.WriteLine($"[Cache] Guild loaded: {arg.Guild.Name} ({arg.Guild.Id})");
        }
        finally
        {
            gate.Release();
        }
    }
}