using EnananBot.Cache;
using EnananBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

// Suppress null warning for Token retrieval
#pragma warning disable CS8602 

// --- QuestPDF Configuration ---
// Set license to Community to comply with free usage terms
QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.EnableDebugging = false;

// Register custom fonts for image generation
string[] fontFiles =
[
    "Inter-Light.ttf",
    "Inter-Regular.ttf",
    "Inter-SemiBold.ttf",
    "Inter-Bold.ttf",

    "NotoSansMiao-Regular.ttf",
    "NotoSansMath-Regular.ttf",
    "NotoEmoji-VariableFont_wght.ttf",
    "NotoSansTC-VariableFont_wght.ttf",
    "NotoSansKR-VariableFont_wght.ttf",
    "NotoSansSC-VariableFont_wght.ttf",
    "NotoSansJP-VariableFont_wght.ttf",

    "NotoSansSymbols-VariableFont_wght.ttf",
    "NotoSansSymbols2-Regular.ttf",
    "NotoSans-VariableFont_wdth,wght.ttf",

    "Shimenkan-Regular.ttf",
    "ShimenkanBook-Regular.ttf",
    "MiaoUnicode-Regular.ttf",

    "unifont-16.0.04.ttf",
    "unifont-SMP-Upper-16.0.04.ttf"
];

foreach (var font in fontFiles)
{
    FontManager.RegisterFont(
        File.OpenRead(Path.Combine("Resources", "Fonts", font)));
}

// Ensure all glyphs render correctly to avoid "tofu" boxes
QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = true;

// --- QuestPDF Warmup ---
// Generate a dummy document to initialize the font cache and rendering engine
// This prevents the first user command from lagging
var warmup = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(20, 20);
        page.Margin(0);
        page.Content()
            .AlignMiddle()
            .AlignCenter()
            .Text("Ena")
            .FontFamily("Inter")
            .FontSize(2);
    });
});

// Execute the generation to trigger the warmup
_ = warmup.GenerateImages();

// --- Bot Host Configuration ---
var builder = Host.CreateApplicationBuilder(args);

// Retrieve the bot token from configuration (appsettings.json or Environment Variables)
var token = builder.Configuration["Discord:Token"] 
            ?? throw new InvalidOperationException("Discord token not configured.");

builder.Services
    // Configure the Discord Gateway (WebSocket connection)
    .AddDiscordGateway(options =>
    {
        options.Token = token;
        // Request all intents (Privileged intents must be enabled in the Discord Developer Portal)
        options.Intents = GatewayIntents.All; 
        options.Presence = new PresenceProperties(UserStatusType.Online)
        {
            Activities = [
                new UserActivityProperties("Custom Status", UserActivityType.Custom)
                    { State = "🎨 Here to paint your world~!" }, 
                new UserActivityProperties("Custom Status", UserActivityType.Custom) 
                    { State = "🖌️ One color at a time…" }]

        };
    })
    // Enable Slash Command support
    .AddApplicationCommands()
    // Register Application Services (Dependency Injection)
    .AddSingleton<GuildCache>()
    .AddSingleton<ValidationService>()
    .AddSingleton<ImageGeneratorService>()
    .AddSingleton<MessageService>()
    .AddSingleton<ImageService>()
    .AddSingleton<RegexGeneratorService>()
    // Register GuildCache as a HostedService so it can run background initialization/tasks
    .AddHostedService(sp => sp.GetRequiredService<GuildCache>())
    .AddHttpClient()
    // Register all event handlers found in the current assembly
    .AddGatewayHandlers(typeof(Program).Assembly);

var host = builder.Build();

// Register all command modules found in the current assembly
host.AddModules(typeof(Program).Assembly);

// Start the application
await host.RunAsync();