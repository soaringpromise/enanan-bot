using EnananBot.Objects;
using EnananBot.Services;
using EnananBot.Utils;
using NetCord.Services.ApplicationCommands;

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace EnananBot.Modules.Commands;

/// <summary>
/// Slash Command Module for Miscellaneous Info.
/// Contains: /credits, /donate
/// Independent of the main database logic.
/// </summary>
public class MiscCommands(MessageService messages)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    /// <summary>
    /// Command: /credits
    /// Displays a rich embed listing the developers, artists, and writers.
    /// Uses 'SendFieldResponse' to organize the data into neat columns/rows.
    /// </summary>
    [SlashCommand("credits", "All the people that made this bot possible!")]
    public async Task CreditsCommand()
    {
        // Define the list of contributors.
        // Format: (Title of the Field, Content of the Field, Inline Boolean)
        var contributors = new List<(string Title, string Info, bool Inline)>
        {
            (
                "Programming",
                // Using custom emoji defined in Objects/Emojis.cs
                $"{Emojis.Twitter} [kii (@soaringpromise)](<https://x.com/soaringpromise>)",
                false // False = Takes up the whole width of the embed row
            ),
            (
                "Writing",
                $"{Emojis.Bluesky} [RamblyngRobyn (@ramblyngrobyn.bsky.social)](<https://bsky.app/profile/ramblyngrobyn.bsky.social>)",
                false
            ),
            (
                "Icon Art",
                $"{Emojis.Twitter} [Xin (@XinChan_)](<https://x.com/XinChan_>)",
                false
            ),
            (
                "Source Code",
                $"{Emojis.GitHub} [enanan-bot on GitHub](<https://github.com/soaringpromise/enanan-bot>)",
                false
            ),
            (
                "Special Thanks",
                $"{Emojis.Discord} [enanan nation ♡ 絵名](<https://discord.gg/X7TBEFeQym>)",
                false
            )
        };

        // Send the formatted embed
        await ResponseUtils.SendFieldResponse(
            Context,
            messages.Credits(), // Fetches the title/description text from the MessageService
            contributors);
    }

    /// <summary>
    /// Command: /donate
    /// Simple command to output a Ko-Fi link.
    /// </summary>
    [SlashCommand("donate", "Support the bot's development.")]
    public async Task DonateCommand()
    {
        var message = messages.Donate("https://ko-fi.com/soaringpromise");
        await ResponseUtils.SendSimpleResponse(Context, message);
    }
    
    /// <summary>
    /// Command: /invite
    /// Simple command to output the bot's permanent invite link.
    /// </summary>
    [SlashCommand("invite", "You want to invite me to your server?!")]
    public async Task InviteCommand()
    {
        var message = messages.Invite("https://discord.com/oauth2/authorize?client_id=1460482352001323185&permissions=4505131052035168&integration_type=0&scope=bot");
        await ResponseUtils.SendSimpleResponse(Context, message);
    }
}