namespace EnananBot.Objects.Enums;

/// <summary>
/// Categories representing the different emotional tones or contexts for bot responses.
/// These are used by EmojiService to select an appropriate random emoji.
/// </summary>
public enum EmojiCategory
{
    /// <summary>
    /// Operation completed successfully (e.g., Role Created).
    /// </summary>
    Success,

    /// <summary>
    /// User input error or validation failure (e.g., Invalid Color).
    /// </summary>
    Failure,

    /// <summary>
    /// System error or critical failure (e.g., Database locked).
    /// </summary>
    Error,

    /// <summary>
    /// Context specific to image generation commands.
    /// </summary>
    Image,

    /// <summary>
    /// General information, help, or neutral context.
    /// </summary>
    Misc
}