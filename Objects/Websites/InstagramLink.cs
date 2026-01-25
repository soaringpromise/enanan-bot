using System.Text.RegularExpressions;
using EnananBot.Services;

namespace EnananBot.Objects.Websites;

/// <summary>
/// Handler for Instagram posts and Reels.
/// Redirects Instagram URLs to 'fxstagram.com' (or 'ddinstagram') to fix broken embeds and video playback.
/// </summary>
public class InstagramLink(string url) : GenericWebsiteLink(url)
{
    // Define the various shapes of Instagram URLs.
    // Supports standard Posts (/p/), Reels (/reels/), and Share links (/share/).
    // "img_index" is captured to support linking to a specific image in a carousel.
    private static readonly Dictionary<string, Regex> Routes =
        RegexGeneratorService.GenerateRoutes(
            ["instagram.com"],
            new Dictionary<string, string[]?>
            {
                // Short share link: /share/12345
                { "/share/:id", ["img_index"] },
                
                // Share link with type: /share/reel/12345
                { "/share/:media_type(p|reels?)/:id", ["img_index"] },
                
                // Standard browser link: /p/12345 or /reels/12345
                { "/:media_type(p|reels?)/:id", ["img_index"] },
                
                // Legacy/User context link: /username/p/12345
                { "/:username/:media_type(p|reels?)/:id", ["img_index"] }
            }
        );

    protected override Dictionary<string, Regex> PossibleRoutes => Routes;
    
    protected override string FixerName => "InstaFix";
    
    // The domain that provides the fixed embed
    protected override string FixDomain => "fxstagram.com";
    
    public override string HypertextLabel => "Instagram";
    
    /// <summary>
    /// Overrides the replacement logic to normalize "Share" links.
    /// Many fixers don't understand /share/, so we convert it to the standard /p/ (post) format.
    /// </summary>
    protected override string GetRepl(string route, Match match) =>
        base.GetRepl(route == "/share/:id" ? "/share/p/:id" : route, match);
    
    public override bool SupportsDomain(string host) =>
        host == "instagram.com";
}