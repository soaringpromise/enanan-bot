namespace EnananBot.Objects.Websites;

/// <summary>
/// A factory class responsible for identifying and creating the appropriate
/// WebsiteLink handler for a given URL.
/// </summary>
public static class WebsiteLinkFactory
{
    // A registry of all available link fixer implementations.
    // When a URL comes in, we check it against each of these in order.
    private static readonly Func<string, WebsiteLink>[] LinkFactories =
    [
        url => new TwitterLink(url),
        url => new InstagramLink(url),
        url => new RedditLink(url),
        url => new TikTokLink(url),
        url => new PixivLink(url)
    ];
    
    /// <summary>
    /// Attempts to create a valid link handler for the provided URL string.
    /// Returns null if the URL is invalid or no handler supports it.
    /// </summary>
    public static WebsiteLink? Create(string url)
    {
        // Safety check: Prevent processing of absurdly long strings (DoS protection)
        if (url.Length > 2048)
            return null;

        // Basic validation: Is it actually a URL?
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            return null;

        // Iterate through all registered factories.
        // The first one that returns a "Valid" link object (meaning the regex matched) wins.
        return LinkFactories
            .Select(factory => factory(url))
            .FirstOrDefault(link => link.IsValid);
    }
}