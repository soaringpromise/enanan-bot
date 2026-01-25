namespace EnananBot.Objects;

/// <summary>
/// A centralized repository for static image asset URLs used by the bot.
/// </summary>
public static class Images
{
    /// <summary>
    /// A list of remote URLs for welcome banners.
    /// The ImageService selects one of these at random when a new user joins.
    /// These are hosted on an external CDN to reduce bot bandwidth/size.
    /// </summary>
    public static readonly string[] WelcomeBanners =
    [
        "https://cdn.soaringpromise.moe/enanan/bot/banners/enawelcome1.webp",
        "https://cdn.soaringpromise.moe/enanan/bot/banners/enawelcome2.webp",
        "https://cdn.soaringpromise.moe/enanan/bot/banners/enawelcome3.webp",
        "https://cdn.soaringpromise.moe/enanan/bot/banners/enawelcome4.webp",
        "https://cdn.soaringpromise.moe/enanan/bot/banners/enawelcome5.webp",
        "https://cdn.soaringpromise.moe/enanan/bot/banners/enawelcome6.webp",
        "https://cdn.soaringpromise.moe/enanan/bot/banners/enawelcome7.webp",
        "https://cdn.soaringpromise.moe/enanan/bot/banners/enawelcome8.webp",
    ];
}