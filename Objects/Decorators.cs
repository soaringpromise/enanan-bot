namespace EnananBot.Objects;

/// <summary>
/// A catalog of special Unicode characters and emojis available for users to decorate their role names.
/// This dictionary is consumed by the AutocompleteService to provide search suggestions.
/// Key = The searchable display name (e.g., "★ Star")
/// Value = The actual character to insert (e.g., "★")
/// </summary>
public static class Decorators
{
    // IReadOnlyDictionary ensures this collection cannot be modified at runtime.
    public static readonly IReadOnlyDictionary<string, string> All = new Dictionary<string, string>
    {
        { "★ Star", "★" },
        { "☆ Hollow Star", "☆" },
        { "✦ Sparkle", "✦" },
        { "✧ Gleam", "✧" },
        { "✪ Starburst", "✪" },
        { "☀ Sun", "☀" },
        { "☾ Moon", "☾" },
        { "❂ Sunburst", "❂" },
        { "☄ Comet", "☄" },
        { "✨ Sparkles", "✨" },
        { "♥︎ Heart", "♥︎" },
        { "𖹭 Hollow Heart", "𖹭" },
        { "❥ Cute Heart", "❥" },
        { "💖 Sparkling Heart", "💖" },
        { "💜 Purple Heart", "💜" },
        { "💛 Yellow Heart", "💛" },
        { "💙 Blue Heart", "💙" },
        { "💚 Green Heart", "💚" },
        { "✿ Flower", "✿" },
        { "❀ Petal", "❀" },
        { "❁ Bloom", "❁" },
        { "✾ Floral", "✾" },
        { "☘ Clover", "☘" },
        { "🌸 Cherry Blossom", "🌸" },
        { "🌺 Hibiscus", "🌺" },
        { "🌼 Blossom", "🌼" },
        { "🌻 Sunflower", "🌻" },
        { "❖ Diamond", "❖" },
        { "◆ Black Diamond", "◆" },
        { "◇ Hollow Diamond", "◇" },
        { "⬥ Rhombus", "⬥" },
        { "⬦ Filled Diamond", "⬦" },
        { "✦ Star Diamond", "✦" },
        { "⚡ Lightning", "⚡" },
        { "♛ Crown", "♛" },
        { "♕ Queen Crown", "♕" },
        { "♚ King Crown", "♚" },
        { "🔥 Fire", "🔥" },
        { "✎ Pencil", "✎" },
        { "✉ Envelope", "✉" },
        { "❉ Crystal", "❉" },
        { "✵ Snowflake", "✵" },
        { "✸ Spark", "✸" },
        { "🎵 Music Note", "🎵" },
        { "🎶 Music Notes", "🎶" }
    };
}