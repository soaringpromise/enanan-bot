using EnananBot.Utils;
using NetCord;
using NetCord.Rest;

namespace EnananBot.Embeds;

/// <summary>
/// A factory class for creating Discord Embeds with a consistent visual theme.
/// Enforces the bot's "Brand Identity" (Colors, Icons, Footers) across all messages.
/// </summary>
public static class EmbedBuilder
{
    // --- Branding Constants ---
    // These define the look and feel of the "Nightcord" app interface the bot mimics.
    private const string IconUrl = "https://cdn.soaringpromise.moe/enanan/bot/ena_icon.png";
    private const string AuthorName = "えななん (@enanan_bot)";
    private const string FooterIconUrl = "https://cdn.soaringpromise.moe/enanan/bot/nightcord.png";
    private const string FooterText = "Via Nightcord App";
    private const string BotUrl = "https://enanan.soaringpromise.moe";
    
    // The signature beige/brown color used by the character.
    private static readonly Color EnaColor = new(0xCCAA88);
    
    /// <summary>
    /// Creates the skeleton of an Embed with all standard branding applied.
    /// This prevents code duplication in the specific methods below.
    /// </summary>
    private static EmbedProperties CreateBase()
    {
        return new EmbedProperties()
            .WithAuthor(
                new EmbedAuthorProperties()
                    .WithIconUrl(IconUrl)
                    .WithName(AuthorName)
                    .WithUrl(BotUrl))
            .WithFooter(
                new EmbedFooterProperties()
                    .WithText(FooterText)
                    .WithIconUrl(FooterIconUrl))
            .WithTimestamp(DateTime.UtcNow) // Always show the current time
            .WithColor(EnaColor);
    }
    
    /// <summary>
    /// Creates a standard text-only response.
    /// Appends fake social media stats (Likes/RTs) to the text.
    /// </summary>
    public static EmbedProperties SimpleMessageEmbed(string message)
    {
        return CreateBase()
            .WithDescription(message.FakeStats());
    }

    /// <summary>
    /// Creates an embed primarily for displaying an image (e.g., Color Previews, Palettes).
    /// </summary>
    /// <param name="imageUrl">URL or attachment:// reference.</param>
    /// <param name="description">Optional caption text.</param>
    public static EmbedProperties ImageEmbed(string imageUrl, string? description = null)
    {
        return description == null 
            ? CreateBase()
                .WithImage(new EmbedImageProperties(imageUrl))
                .WithDescription(string.Empty.FakeStats()) // Even empty descriptions get stats
            : CreateBase()
                .WithImage(new EmbedImageProperties(imageUrl))
                .WithDescription(description.FakeStats());
    }
    
    /// <summary>
    /// Creates an embed with structured data fields (e.g., Help commands, User Info).
    /// </summary>
    /// <param name="message">The main body text.</param>
    /// <param name="fieldData">A list of tuples containing (Title, Content, Inline-Boolean).</param>
    public static EmbedProperties FieldEmbed(
        string message,
        IEnumerable<(string Name, string Value, bool Inline)> fieldData)
    {
        // Convert the simple Tuple data into NetCord's EmbedFieldProperties objects
        var fields = fieldData.Select(field =>
            new EmbedFieldProperties()
                .WithName(field.Name)
                .WithValue(field.Value)
                .WithInline(field.Inline));

        return CreateBase()
            .WithDescription(message.FakeStats())
            .WithFields(fields);
    }
}