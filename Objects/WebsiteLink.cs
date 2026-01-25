namespace EnananBot.Objects;

// ReSharper disable UnusedMember.Global

/// <summary>
/// Abstract base class representing a fixable website link.
/// Implementations of this class handle specific logic for platforms (e.g., Twitter, TikTok, Pixiv).
/// </summary>
/// <param name="url">The raw URL string found in the user's message.</param>
public abstract class WebsiteLink(string url)
{
    /// <summary>
    /// The original raw URL provided by the user.
    /// </summary>
    protected string Url { get; } = url;
    
    /// <summary>
    /// The display name for the link in the final message (e.g., "Twitter", "Pixiv").
    /// </summary>
    public abstract string HypertextLabel { get; }
    
    /// <summary>
    /// Determines if the specific URL path/parameters are valid for fixing.
    /// (e.g., a Twitter link is only valid if it points to a specific status, not the homepage).
    /// </summary>
    public abstract bool IsValid { get; }
    
    /// <summary>
    /// Checks if this handler supports the given domain (hostname).
    /// Used by the factory to select the correct handler.
    /// </summary>
    /// <param name="host">The hostname from the URL (e.g., "twitter.com").</param>
    public abstract bool SupportsDomain(string host);
    
    /// <summary>
    /// Asynchronously generates the "Fixed" embed-friendly URL.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// <br/>- FixedUrl: The new URL (e.g., fxtwitter.com/...)
    /// <br/>- FixerName: The name of the service used (e.g., "FxTwitter")
    /// </returns>
    public abstract Task<(string? FixedUrl, string? FixerName)> GetFixedUrlAsync();
}