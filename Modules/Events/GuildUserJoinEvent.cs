using System.Collections.Concurrent;
using EnananBot.Cache;
using EnananBot.Embeds;
using EnananBot.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Hosting.Gateway;

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace EnananBot.Modules.Events;

/// <summary>
/// Event handler triggered when a new user joins a server.
/// Responsible for registering the user in the database and sending a welcome message if configured.
/// </summary>
public class GuildUserJoinEvent(
    GuildCache guildCache,
    RestClient client,
    MessageService messages,
    ImageService images)
    : IGuildUserAddGatewayHandler
{
    // Thread-safe locking mechanism to prevent race conditions when multiple users join the same server simultaneously
    private static readonly ConcurrentDictionary<ulong, SemaphoreSlim> GuildLocks = new();

    public async ValueTask HandleAsync(GuildUser user)
    {
        // Ignore bots to keep the database clean and avoid welcoming other bots
        if (user.IsBot) return;

        // Get the lock specific to this server
        var gate = GuildLocks.GetOrAdd(user.GuildId, _ => new SemaphoreSlim(1, 1));

        await gate.WaitAsync();

        try
        {
            // 1. Register User in Database
            var wasAdded = await guildCache.AddUserAsync(user.GuildId, user.Id);

            Console.WriteLine(wasAdded
                ? $"[Cache] New user registered: {user.Username} ({user.Id}) in {user.GuildId}"
                : $"[Cache] User already exists: {user.Id} in {user.GuildId}");

            // 2. Check for Welcome Channel Configuration
            var welcomeChannelId =
                await guildCache.GetWelcomeChannelAsync(user.GuildId);

            // If no channel is set up for this server, stop here
            if (!welcomeChannelId.HasValue)
                return;

            // Verify the channel actually exists and is a Text Channel
            if (await client.GetChannelAsync(welcomeChannelId.Value)
                is not TextGuildChannel channel)
                return;
            
            // Format the message to mention the user
            var welcomeText = messages.Welcome(user.Id);

            // A brief delay to allow Discord UI to update or AutoMod bots to run checks
            // Helps prevent the "ghost ping" effect if the user is kicked immediately by another bot
            await Task.Delay(TimeSpan.FromSeconds(3));

            // Select a random image from the CDN list
            var bannerUrl = images.PickWelcomeBanner();

            // Build and send the embed
            var embed = EmbedBuilder.ImageEmbed(bannerUrl, welcomeText);

            await channel.SendMessageAsync(new MessageProperties().AddEmbeds(embed));
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Error] Failed to process joining user {user.Id} in {user.GuildId}: {e}");
        }
        finally
        {
            // ALWAYS release the lock
            gate.Release();
        }
    }
}