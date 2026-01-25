using System.Text.RegularExpressions;
using EnananBot.Services;

namespace EnananBot.Objects.Websites;

/// <summary>
/// Handler for Reddit links.
/// Redirects Reddit URLs to 'vxreddit.com' to fix broken video embeds and galleries.
/// </summary>
public class RedditLink(string url) : GenericWebsiteLink(url)
{
    // Regex definitions for the various shapes of Reddit URLs.
    private static readonly Dictionary<string, Regex> Routes =
        RegexGeneratorService.GenerateRoutes(
            ["reddit.com", "redditmedia.com"],
            new Dictionary<string, string[]?>
            {
                // Standard Post: /r/funny/comments/123abc_slug_here
                // Also handles User posts: /u/username/comments/...
                { "/:post_type(u|r|user)/:username/:type(comments|s)/:id/:slug?", null },
                
                // Deep link to a specific comment chain
                { "/:post_type(u|r|user)/:type(comments|s)/:id/:slug/:comment", null },
                
                // Shortlinks or raw ID references
                { "/:id", null }
            }
        );

    protected override Dictionary<string, Regex> PossibleRoutes => Routes;
    
    // The name of the service (displayed in the footer of the fixed message)
    protected override string FixerName => "vxreddit";
    
    // The domain that provides the fixed embed
    protected override string FixDomain => "vxreddit.com";
    
    public override string HypertextLabel => "Reddit";

    /// <summary>
    /// Explicitly checks strictly for reddit.com domains.
    /// Note: Does not include 'redd.it' shortlinks here, likely handled by the /:id route 
    /// if the hostname was passed in, but strictly filtering to standard domains for now.
    /// </summary>
    public override bool SupportsDomain(string host) =>
        host is "reddit.com" or "www.reddit.com" or "redditmedia.com";
}