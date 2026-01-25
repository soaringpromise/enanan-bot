using EnananBot.Objects;
using EnananBot.Objects.Enums;

namespace EnananBot.Services;

/// <summary>
/// A static utility service for retrieving random emoji based on the context of the message.
/// Used to add variety to the bot's responses.
/// </summary>
public static class EmojiService
{
    private static readonly Random Rng = Random.Shared;

    /// <summary>
    /// Selects a random emoji string from the pool corresponding to the specified category.
    /// </summary>
    /// <param name="category">The category of the response (e.g., Success, Error, Image).</param>
    /// <returns>A string containing the Discord emoji code.</returns>
    public static string Pick(EmojiCategory category)
    {
        // Map the abstract category to the specific array of emoji strings
        var pool = category switch
        {
            EmojiCategory.Success => Emojis.EnaSuccess,
            EmojiCategory.Failure => Emojis.EnaFailure,
            EmojiCategory.Error   => Emojis.EnaError,
            EmojiCategory.Image   => Emojis.EnaImage,
            _                     => Emojis.EnaMisc
        };

        // return a random emoji from the selected pool
        return pool[Rng.Next(pool.Length)];
    }
}