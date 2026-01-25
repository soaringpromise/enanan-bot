using System.Text.RegularExpressions;
using EnananBot.Services;

namespace EnananBot.Objects.Websites;

/// <summary>
/// Handler for Twitter and X.com links.
/// Redirects valid tweet URLs to 'fxtwitter.com' to fix Discord embeds (video playback, images, etc.).
/// </summary>
public class TwitterLink(string url) : GenericWebsiteLink(url)
{
    // Define the specific URL patterns we want to match.
    // We only care about actual posts (status), not profiles or settings pages.
    private static readonly Dictionary<string, Regex> Routes =
        RegexGeneratorService.GenerateRoutes(
            ["twitter.com", "x.com"],
            new Dictionary<string, string[]?>
            {
                // Old mobile style: twitter.com/i/status/12345
                { "/i/status/:id", null },
                // Standard style: twitter.com/username/status/12345
                { "/:username/status/:id", null }
            });

    protected override string FixerName => "FxTwitter";
    
    // The service that provides the fixed embed (fxtwitter.com)
    protected override string FixDomain => "fxtwitter.com";
    
    public override string HypertextLabel => "Twitter";
    
    protected override Dictionary<string, Regex> PossibleRoutes => Routes;
    
    /// <summary>
    /// Fast check to see if this factory supports the incoming domain.
    /// </summary>
    public override bool SupportsDomain(string host) =>
        host is "twitter.com" or "x.com";
    
    /// <summary>
    /// Overrides the generic replacement logic to ensure the output URL is perfectly formatted.
    /// </summary>
    protected override string GetRepl(string route, Match match)
    {
        return route switch
        {
            // If the input was "/i/status/123", rebuild it manually
            "/i/status/:id" =>
                $"https://{FixDomain}/i/status/{match.Groups["id"].Value}",
            
            // If the input was "/username/status/123", rebuild it manually
            "/:username/status/:id" =>
                $"https://{FixDomain}/{match.Groups["username"].Value}/status/{match.Groups["id"].Value}",
            
            // Fallback to the generic logic for anything else
            _ => base.GetRepl(route, match)
        };
    }
}