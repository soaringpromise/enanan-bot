namespace EnananBot.Objects;

/// <summary>
/// A registry of Discord custom emoji strings.
/// These strings follow the format &lt;:name:id&gt;, which Discord parses into images.
/// The arrays are used by EmojiService to pick random variations for the bot's "personality".
/// </summary>
public static class Emojis
{

    // Used in footers, credits, or specific UI elements
    public const string Twitter = "<:twitter:1460877444876861470>";
    public const string Discord = "<:discord:1460877434990891069>";
    public const string GitHub = "<:github:1460877442632646812>";
    public const string Bluesky = "<:bluesky:1460877433451581569>";

    // --- Easter Eggs ---
    // Rare joke emoji that might appear as a reaction to a message
    public static readonly (string Name, ulong Id)[] EasterEggs =
    [
        ("nightcordat2500", 1462355324387463168),
        ("nooooat2500", 1462355326442405930)
    ];
    
    // --- Response Variations ---
    // Arrays allow the bot to randomly select an emoji for a given category

    // Used for successful operations
    public static readonly string[] EnaSuccess =
    [
        "<:enasuccess1:1464108263833341962>",
        "<:enasuccess2:1464108267184718077>",
        "<:enasuccess3:1464108269470617757>",
        "<:enasuccess4:1464108272100311192>",
        "<:enasuccess5:1464108274906562707>",
        "<:enasuccess6:1464108283181797544>"
    ];

    // Used for user-errors or validation failures
    public static readonly string[] EnaFailure =
    [
        "<:enafailure1:1464107405855031317>",
        "<:enafailure2:1464107408967209031>",
        "<:enafailure3:1464107411551027261>",
        "<:enafailure4:1464107413715292261>",
        "<:enafailure5:1464107416483270697>",
        "<:enafailure6:1464107418459045921>",
        "<:enafailure7:1464107421684465675>",
        "<:enafailure8:1464107425496961024>",
        "<:enafailure9:1464107430488314059>",
        "<:enafailure10:1464107432484671518>"
    ];

    // Used for system crashes or critical failures
    public static readonly string[] EnaError =
    [
        "<:enaerror1:1464106995559829607>",
        "<:enaerror2:1464106998223081504>",
        "<:enaerror3:1464106999750066290>",
        "<:enaerror4:1464107001587175518>",
        "<:enaerror5:1464107003797573695>",
        "<:enaerror6:1464107006687318037>",
        "<:enaerror7:1464107009442975774>"
    ];

    // Used specifically for image generation/preview commands
    public static readonly string[] EnaImage = 
    [
        "<:enaimage1:1464107857543827713>",
        "<:enaimage2:1464107859599036516>",
        "<:enaimage3:1464107863289892995>",
        "<:enaimage4:1464107866142281859>",
        "<:enaimage5:1464107868050554940>",
        "<:enaimage6:1464107872932859914>"
    ];

    // Used for general info, help, or neutral messages
    public static readonly string[] EnaMisc =
    [
        "<:enamisc1:1464108061059846164>",
        "<:enamisc2:1464108063525961738>",
        "<:enamisc3:1464108066806038613>",
        "<:enamisc4:1464108068915904594>",
        "<:enamisc5:1464108071738671186>",
        "<:enamisc6:1464108074364305640>",
        "<:enamisc7:1464108076503404556>"
    ];
}