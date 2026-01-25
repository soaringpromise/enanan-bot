using System.Text.RegularExpressions;
using EnananBot.Services;

namespace EnananBot.Objects.Websites;

/// <summary>
/// Handler for TikTok links.
/// Redirects TikTok URLs to 'tnktok.com' (or similar services) to fix embed playback.
/// Handles logic for distinguishing between standard videos and photo slideshows.
/// </summary>
public class TikTokLink(string url) : GenericWebsiteLink(url)
{
    // Define the various URL patterns TikTok uses.
    // 1. Standard: /@username/video/12345 (or /photo/12345)
    // 2. Shortlinks/Embeds: /t/12345 or /embed/12345
    // 3. Raw IDs (rare but possible in some contexts)
    private static readonly Dictionary<string, Regex> Routes =
        RegexGeneratorService.GenerateRoutes(
            ["tiktok.com"],
            new Dictionary<string, string[]?>
            {
                { "/@:username/:media_type(video|photo)/:id", null },
                { "/:shortlink_type(t|embed)/:id", null },
                { "/:id", null }
            });
    
    protected override Dictionary<string, Regex> PossibleRoutes => Routes;
    
    protected override string FixerName => "fxTikTok";
    
    // The base domain for the fixer service
    protected override string FixDomain => "tnktok.com";
    
    public override string HypertextLabel => "TikTok";
    
    private const string DefaultView = "Normal";
    
    /// <summary>
    /// Checks for any subdomain ending in tiktok.com (e.g., www, vm, vt).
    /// </summary>
    public override bool SupportsDomain(string host) =>
        host.EndsWith("tiktok.com");
    
    // Mapping for specific subdomains required by the fixer service to render correctly.
    private static readonly Dictionary<string, string> Subdomains = new()
    {
        { "Normal", "a." },       // Standard video view
        { "Gallery", "" },        // Photo/Slideshow view (no subdomain)
        { "DirectMedia", "d." }   // Direct file access
    };
    
    /// <summary>
    /// Dynamically selects the subdomain based on the content type.
    /// </summary>
    protected override string GetSubdomain(Match match)
    {
        // If the URL explicitly says "photo", use the Gallery subdomain (empty)
        // This ensures slideshows render as scrollable images rather than a broken video.
        if (match.Groups["media_type"].Value == "photo")
            return Subdomains["Gallery"];
        
        // If it's a video, use the "DirectMedia" or "Normal" subdomain logic.
        return Url.Contains("/video/")
            ? Subdomains["DirectMedia"]
            : Subdomains["Normal"];
    }
    
    protected override string RouteFixSubdomain() =>
        Subdomains.GetValueOrDefault(DefaultView, "");
}