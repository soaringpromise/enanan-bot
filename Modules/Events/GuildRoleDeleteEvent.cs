using EnananBot.Cache;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace EnananBot.Modules.Events;

/// <summary>
/// Event handler triggered when a Role is deleted in a server.
/// Responsible for cleaning up database references if the deleted role was a "User Role" managed by the bot.
/// </summary>
public class GuildRoleDeleteEvent(GuildCache guildCache) : IRoleDeleteGatewayHandler
{
    public async ValueTask HandleAsync(RoleDeleteEventArgs arg)
    {
        try
        {
            // If the deleted role ID matches any UserID key in our database, 
            // we must remove that entry so the user can create a new role later
            await guildCache.ClearRoleAsync(arg.GuildId, arg.RoleId);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Error] Failed to clear role {arg.RoleId} in guild {arg.GuildId}: {e}");
        }
    }
}