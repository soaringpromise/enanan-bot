namespace EnananBot.Messages;

/// <summary>
/// A static container for dialogue lines related to Color commands.
/// </summary>
public static class ColorMessages
{
    // Responses for /color preview success
    // {0} = Username
    public static readonly string[] MessagePreviewSuccess =
    [
        "Got it! Let me read over this real quick…",
        "Let me take a look at this, **{0}**.",
        "Okay… yeah. I see it now."
    ];

    // Responses for /color preview failure
    // {0} = Username
    public static readonly string[] MessagePreviewError =
    [
        "I didn't get that message, **{0}**. Try sending it again?",
        "Could you shoot me that one more time?",
        "Wait, what did you just send, **{0}**? I can't see it."

    ];

    // Responses for /color palette success
    // {0} = Username
    public static readonly string[] ColorPaletteSuccess =
    [
        "Alright, here's a palette I made. Hopefully it's to your liking.",
        "Hey, I got that palette for you, **{0}**… Do you like it?",
        "Does this look okay, **{0}**? I made you a color palette like you asked."

    ];

    // Responses for generic failures in color processing
    public static readonly string[] ColorPaletteError =
    [
        "Sorry, I don't think I can make that work.",
        "Er, could I take another shot at that?",
        "Gimme a sec, I'm working on something else right now.",
        "Uh… my brain is not cooperating with colors right now."

    ];

    // Responses for /color list
    // {0} = Username, {1} = Link to the website
    public static readonly string[] ListAllColors =
    [
        "**{0}**, you want me to list… EVERY color? Yeah, okay. That's not happening. Just check this out instead: <{1}>",
        "Hey, wait a second **{0}**! I'm not crazy enough to list all 148 colors myself. I already have something for that. Here: <{1}>",
        "**{0}**… I am not typing out a color encyclopedia by hand. I have standards. Anyway, here: <{1}>"
    ];
}