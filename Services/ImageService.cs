using EnananBot.Objects;

namespace EnananBot.Services;

/// <summary>
/// Service responsible for managing and retrieving static image assets (URLs or paths).
/// </summary>
public sealed class ImageService
{
    private readonly Random _random = Random.Shared;

    /// <summary>
    /// Selects a random welcome banner URL from the predefined collection.
    /// Used when generating the welcome image or embed.
    /// </summary>
    public string PickWelcomeBanner()
        => Images.WelcomeBanners[_random.Next(Images.WelcomeBanners.Length)];
}