namespace EnananBot.Messages;

/// <summary>
/// A static container for dialogue lines related to Role Management.
/// </summary>
public static class RoleMessages
{
    // --- CREATE ---

    // Success: {0} = Username
    public static readonly string[] CreateSuccess =
    [
        "Got it! Another role added to the list. Like it, **{0}**?",
        "Sure are adding a lot of roles today, huh?",
        "Alright, alright, one more role…"
    ];

    // Error: {0} = Username
    public static readonly string[] CreateError =
    [
        "Ugh, sorry, **{0}**. Something went wrong.",
        "I got a saving error?! UGH.",
        "Why does it always break when I'm doing fine for once…?"
    ];

    // --- EDIT ---

    // Success: {0} = Username
    public static readonly string[] EditSuccess =
    [
        "Here's the touch ups you asked for, **{0}**!",
        "And… done!",
        "Okay, yeah, that *does* look better now."
    ];

    // Error: {0} = Username
    public static readonly string[] EditError =
    [
        "Sorry, that's not gonna work, **{0}**. Any better ideas?",
        "I don't think that saved properly… Ugh.",
        "Nope. Still wrong. I swear I didn't mess it up this time."
    ];

    // --- DELETE ---

    // Success: {0} = Username
    public static readonly string[] DeleteSuccess =
    [
        "Fine, trashed it for you, **{0}**.",
        "Well, the role list was getting pretty long, I guess.",
        "Role gone. Try not to miss it too much."
    ];

    // Error: {0} = Username
    public static readonly string[] DeleteError =
    [
        "I couldn't find the stupid trash can! Ugh!",
        "Can we try that again, **{0}**? It's still there on my end.",
        "Looks like that didn't work. Let's get in touch with an admin and see if they can help?"
    ];
    
    // Error: {0} = Username
    public static readonly string[] RoleEditNoChanges =
    [
        "…It looks the same to me, **{0}**.",
        "I don't think anything actually changed?",
        "Uh… did you mean to update something, **{0}**?",
        "I checked, but it's still exactly the same.",
        "You didn't really edit it, but… okay?",
        "If you wanted it to stay like this, you didn't have to ask me.",
        "Nothing new happened here, **{0}**.",
        "Try changing something first, then I can fix it for you."
    ];
}