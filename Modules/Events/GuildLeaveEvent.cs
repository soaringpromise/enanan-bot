using EnananBot.Cache;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace EnananBot.Modules.Events;

/// <summary>
/// Event handler triggered when the bot leaves (or is kicked from) a server.
/// Performs a complete cleanup of that server's data from the database.
/// </summary>
public class GuildLeaveEvent(GuildCache guildCache) : IGuildDeleteGatewayHandler
{
    public async ValueTask HandleAsync(GuildDeleteEventArgs arg)
    {
        try
        {
            Console.WriteLine($"[Cache] Guild removed: {arg.GuildId}");
            
            // Wipe all configuration and user roles for this server
            // This prevents the database from filling up with "dead" data
            await guildCache.RemoveGuildAsync(arg.GuildId);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Error] Failed to remove guild {arg.GuildId}: {e}");
        }
    }
}