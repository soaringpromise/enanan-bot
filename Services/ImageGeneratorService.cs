using EnananBot.Utils;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EnananBot.Services;

/// <summary>
/// Service responsible for generating dynamic images using the QuestPDF layout engine.
/// Instead of saving PDFs, this service renders the document to a byte array (WebP format) for Discord uploads.
/// </summary>
public sealed class ImageGeneratorService
{
    // Theme constants imitating Discord's various theme modes (Light, Dark, Midnight/Onyx)
    private const string ColorLight = "#FBFBFB";
    private const string ColorAsh   = "#323339";
    private const string ColorDark  = "#1A1A1E";
    private const string ColorOnyx  = "#070709";

    private const string TextLight     = "#DCDCDF";
    private const string TextDark      = "#323339";
    private const string TextTimestamp = "#9D9EA5";

    // Flavor text used in the preview images
    private static readonly string[] Lines =
    [
        "Was this the right choice?",
        "Where did I take a wrong turn?",
        "This warmth in my chest…",
        "Why is it not cooling down?"
    ];

    // Default settings for the image generation process
    private static readonly ImageGenerationSettings DefaultSettings = new()
    {
        RasterDpi = 144,
        ImageFormat = ImageFormat.Webp,
        ImageCompressionQuality = ImageCompressionQuality.Medium
    };
    
    /// <summary>
    /// Generates a composite image showing how a user's name and role color look against 
    /// four different background themes (Light, Ash, Dark, Onyx).
    /// </summary>
    /// <param name="username">The display name to render.</param>
    /// <param name="roleColor">The hex color code of the user's role.</param>
    /// <param name="avatarImageBytes">The raw bytes of the user's avatar image.</param>
    /// <returns>A byte array containing the generated WebP image.</returns>
    public byte[] GenerateNamePreview(
        string username,
        string roleColor,
        byte[] avatarImageBytes)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // Define global font styles
                var textStyle = TextStyle.Default.FontFamily(
                    "Lato",
                    "Noto Sans",
                    "Noto Sans Math",
                    "Noto Sans Symbols",
                    "Noto Sans Symbols 2",
                    "Noto Sans JP",
                    "Noto Sans SC",
                    "Noto Sans TC",
                    "Noto Sans KR",
                    "Noto Sans Miao",
                    "Noto Emoji",
                    "Shimenkan",
                    "Shimenkan Book",
                    "Miao Unicode",
                    "Unifont Upper",
                    "Unifont"
                );
                page.DefaultTextStyle(textStyle);
                
                page.ContinuousSize(600); // Width is fixed at 600px
                page.Margin(12);
                page.PageColor(Colors.Transparent); // Transparent background for the image file itself
                
                page.Content().Column(col =>
                {
                    col.Spacing(12);
                    // Rounded corners for the whole stack
                    col.Item().CornerRadius(20);
                    
                    // Render 4 mock messages, one for each background theme
                    col.Item().Element(e => RenderFakeMessage(
                        e, ColorLight, TextDark, username, roleColor, avatarImageBytes, 0));
                    col.Item().Element(e => RenderFakeMessage(
                        e, ColorAsh, TextLight, username, roleColor, avatarImageBytes, 1));
                    col.Item().Element(e => RenderFakeMessage(
                        e, ColorDark, TextLight, username, roleColor, avatarImageBytes, 2));
                    col.Item().Element(e => RenderFakeMessage(
                        e, ColorOnyx, TextLight, username, roleColor, avatarImageBytes, 3));
                });
            });
        });

        // Convert the PDF layout directly to an image
        return document.GenerateImages(DefaultSettings).First();
    }
    
    /// <summary>
    /// Generates a grid visualization of a list of colors.
    /// Useful for visualizing palette commands.
    /// </summary>
    /// <param name="colors">A list of Hex color codes.</param>
    /// <returns>A WebP image byte array.</returns>
    public byte[] GeneratePaletteImage(string[] colors)
    {
        return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(600, 600);
                    page.Margin(10);
                    page.PageColor(Colors.Transparent);

                    // Use a Table for a grid layout (3 columns wide)
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        foreach (var color in colors)
                        {
                            table.Cell()
                                .Padding(6)
                                .Height(180)
                                .CornerRadius(24)
                                .Background(color)
                                .AlignBottom()
                                .AlignRight()
                                .Padding(12)
                                .Text(color) // Display the hex code
                                .FontColor(ColorUtils.GetContrastColor(color)) // Auto-switch text color (black/white) for readability
                                .FontFamily("Comic Neue")
                                .Bold()
                                .FontSize(20);
                        }
                    });
                });
            })
            .GenerateImages(DefaultSettings).First();
    }
    
    /// <summary>
    /// Helper method to render a single "Fake Discord Message" component.
    /// Uses a Row (Avatar and Column (Header and Content)) layout strategy.
    /// </summary>
    private static void RenderFakeMessage(
        IContainer container,
        string bgHex,
        string textHex,
        string username,
        string roleColor,
        byte[] avatar,
        int index)
    {
        const int avatarSize = 45;

        container
            .CornerRadius(8)
            .Background(bgHex)
            .Padding(16)
            .Row(row =>
            {
                row.Spacing(16);
                
                // Left side: User Avatar
                row.ConstantItem(avatarSize)
                    .Width(avatarSize)
                    .AspectRatio(1)
                    .CornerRadius(avatarSize / 2f) // Circular mask
                    .Image(avatar);

                // Right side: Username, Timestamp, and Message text
                row.RelativeItem().Column(col =>
                {
                    // Header Row: Username + Date
                    col.Item().PaddingBottom(2).Row(header =>
                    {
                        header.Spacing(8);

                        // Username
                        header.AutoItem()
                            .MinHeight(20)
                            .AlignBottom() // Align text to bottom to match timestamp baseline
                            .Text(username)
                            .LineHeight(1.2f)
                            .FontColor(roleColor)
                            .FontSize(16)
                            .SemiBold()
                            .FontFamily("Inter");

                        // Timestamp (BOT tag or Date)
                        header.AutoItem()
                            .MinHeight(20)
                            .AlignBottom()
                            .PaddingBottom(2)
                            .Text("Today at 25:00") // "25:00" is a stylistic choice?
                            .FontColor(TextTimestamp)
                            .FontSize(12)
                            .FontFamily("Inter");
                    });
                    
                    // Message Body
                    col.Item()
                        .Text(Lines[index])
                        .FontColor(textHex)
                        .FontSize(15)
                        .FontFamily("Inter")
                        .Light();
                });
            });
    }
}