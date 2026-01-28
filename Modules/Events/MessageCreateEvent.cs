using System.Text.RegularExpressions;
using EnananBot.Objects;
using EnananBot.Objects.Websites;
using EnananBot.Services;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace EnananBot.Modules.Events;

/// <summary>
/// The main event listener for new messages.
/// Triggers whenever a user sends a message in a channel the bot can see.
/// </summary>
public class MessageCreateEvent(MessageService messages) : IMessageCreateGatewayHandler
{
    // A compiled regex to quickly find HTTP/HTTPS links in text
    // Includes a timeout to prevent ReDoS
    private static readonly Regex UrlRegex =
        new(@"https?://[^|\s]+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromMilliseconds(200));

    private static readonly Random Rng = new();

    /// <summary>
    /// The entry point called by NetCord when a message is received.
    /// </summary>
    public async ValueTask HandleAsync(Message msg)
    {
        // 1. Ignore other bots
        // 2. Ignore empty messages or messages without body (e.g.: just an image)
        if (msg.Author.IsBot || string.IsNullOrWhiteSpace(msg.Content))
            return;
        
        // Fire-and-forget logic (awaited but independent)
        await TryEasterEggAsync(msg);
        await TryFixLinksAsync(msg);
    }
    
    /// <summary>
    /// Scans the message for fixable links (Twitter, TikTok, etc.) and replaces them.
    /// </summary>
    private async Task TryFixLinksAsync(Message msg)
    {
        var matches = UrlRegex.Matches(msg.Content);
        if (matches.Count == 0) return;

        foreach (Match match in matches)
        {
            var originalUrl = match.Value;
            
            // We need to know if the user wrapped the link in ||spoilers|| so we can spoiler the fixed link too
            var isSpoilered = false;
            
            var startIndex = match.Index;
            var endIndex = startIndex + match.Length;
            
            // Look for the last "||" appearing BEFORE the URL
            var prefixSpan = msg.Content.AsSpan(0, startIndex);
            var lastSpoilerOpen = prefixSpan.LastIndexOf("||");
            
            // Look for the first "||" appearing AFTER the URL
            var suffixSpan = msg.Content.AsSpan(endIndex);
            var firstSpoilerClose = suffixSpan.IndexOf("||");
            
            // If we found both an opener and a closer...
            if (lastSpoilerOpen != -1 && firstSpoilerClose != -1)
            {
                var closeIndexGlobal = endIndex + firstSpoilerClose;
                
                // Check the text strictly between the last opener and the first closer
                // If there are NO other "||" inside, then our URL is successfully wrapped
                var contentBetween = msg.Content.AsSpan(
                    lastSpoilerOpen + 2, closeIndexGlobal - (lastSpoilerOpen + 2));
                
                if (!contentBetween.Contains("||", StringComparison.Ordinal))
                    isSpoilered = true;
            }
            // Ask the factory if this specific URL is supported
            var linkFixer = WebsiteLinkFactory.Create(originalUrl);
            if (linkFixer == null) continue;

            // Generate the new "fixed" URL (e.g. fxtwitter.com/...)
            var (fixedUrl, fixerName) = await linkFixer.GetFixedUrlAsync();
            if (string.IsNullOrEmpty(fixedUrl)) continue;

            var formattedMessage = isSpoilered
                ? $"|| [**{linkFixer.HypertextLabel}**](<{originalUrl}>) ⟶ [**{fixerName}**]({fixedUrl}) ||"
                : $"[**{linkFixer.HypertextLabel}**](<{originalUrl}>) ⟶ [**{fixerName}**]({fixedUrl})";

            try
            {
                // Reply to the user with the fixed link
                await msg.ReplyAsync(formattedMessage);
                
                // We try to remove the embed from the USER'S original message so we don't have duplicates
                // e.g., We don't want the broken Twitter embed AND the fixed FxTwitter embed visible at the same time
                try
                {
                    await msg.ModifyAsync(m => m.Flags = MessageFlags.SuppressEmbeds);
                }
                catch (RestException e) when (e.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    // If we lack permissions to manage messages, ignore it
                    // The link fix will still work, it will just look a bit cluttered
                }
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LINK REWRITER]: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Attempts to add a random easter egg emoji reaction to a message.
    /// </summary>
    private static async Task TryEasterEggAsync(Message msg)
    {
        // 827 referencing Mizuki Akiyama's birthday, August 27th
        if (Rng.Next(827) != 0) return;

        var emoji = Emojis.EasterEggs;
        const double weight = 0.7;

        // Smaller chance of getting a slightly more humourous reaction
        var chosen = Rng.NextDouble() < weight ? emoji[0] : emoji[1];

        try
        {
            // Add the emoji reaction by name and ID
            await msg.AddReactionAsync(new ReactionEmojiProperties(chosen.Name, chosen.Id));
        }
        catch
        {
            // Usually fails if the bot lacks "Add Reactions" permission
        }
    }
}