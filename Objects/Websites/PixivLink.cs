using System.Text.RegularExpressions;
using EnananBot.Services;

namespace EnananBot.Objects.Websites;

/// <summary>
/// Handler for Pixiv art community links.
/// Redirects Pixiv URLs to 'phixiv.net' to bypass hotlink protection and display full-res art in Discord.
/// </summary>
public class PixivLink(string url) : GenericWebsiteLink(url)
{
    // Pixiv has two distinct URL structures:
    // 1. Legacy PHP: pixiv.net/member_illust.php?illust_id=12345
    // 2. Modern Route: pixiv.net/en/artworks/12345
    private static readonly Dictionary<string, Regex> Routes =
        RegexGeneratorService.GenerateRoutes(
            ["pixiv.net"],
            new Dictionary<string, string[]?>
            {
                // Matches the legacy query parameter style.
                // The "illust_id" string tells the regex generator to look for ?illust_id=...
                { "/member_illust.php", ["illust_id"] },
                
                // Matches the modern path-based style.
                // :lang? handles optional language codes like /en/
                { "/:lang?/artworks/:id/:media?", null }
            }
        );
    
    protected override Dictionary<string, Regex> PossibleRoutes => Routes;
    
    protected override string FixerName => "phixiv";
    
    // The domain that proxies the image
    protected override string FixDomain => "phixiv.net";
    
    public override string HypertextLabel => "Pixiv";
    
    public override bool SupportsDomain(string host) =>
        host == "pixiv.net";
}