namespace EnananBot.Messages;

/// <summary>
/// A static container for System-level dialogue lines.
/// Includes: Setup status, Registration checks, DM errors, and Welcome Channel configuration.
/// </summary>
public static class SystemMessages
{
    // Generic "Something exploded" messages
    // Used when an unhandled exception occurs
    public static readonly string[] GenericError =
    [
        "Sorry, what?",
        "Huh?",
        "I… don't know what happened there.",
        "For goodness sake…",
        "I'll… ask Mizuki about that one."
    ];

    // --- SETUP COMMANDS ---

    // Success for /enanan setup
    // {0} = Admin Name
    public static readonly string[] ServerSetupSuccess =
    [
        "Got myself set up in the server for ya! Check it out!",
        "Thanks for waiting, **{0}**. I'm set up now in your server now.",
        "All done! Everything should be up and running now."
    ];

    // Failure for /enanan setup (e.g., Database timeout)
    public static readonly string[] ServerSetupError =
    [
        "Hey, I couldn't set up shop in your server. Sorry about that.",
        "I'm not sure what just happened, **{0}**, but the server died before it could even get my stuff off the ground!",
        "Something went wrong while I was setting things up… yeah, that one's on me."
    ];
    
    // --- DM HANDLING ---

    // Success for /enanan list (User list sent to DMs)
    public static readonly string[] MessageSentToDMs =
    [
        "Let me just send you that user list. Check your messages!",
        "Got it, sending that your way now. Check our DMs.",
        "Sent! It should be sitting in your DMs right now."
    ];

    // Failure: User has "Allow Direct Messages from Server Members" turned off
    public static readonly string[] AdminDMsAreClosed =
    [
        "So I tried to send you a message, but it didn't work. Do you have your DMs closed or something?",
        "Hey, could you make sure your DMs are open? I wanted to send you something.",
        "I can't get through to your DMs at all. Kinda hard to send things like this."
    ];

    // --- REGISTRATION CHECKS ---

    // Prevent duplicate setup
    public static readonly string[] ServerIsAlreadyRegistered =
    [
        "Hey, the server's already on the list. Just letting you know.",
        "I'm not gonna register your server a second time. Come on now.",
        "This place is already registered, so we're good."
    ];

    // Gatekeeping: Prevents /role commands in unregistered servers
    public static readonly string[] ServerIsNotRegistered =
    [
        "I don't see the server on the list. Are you sure you're in that one?",
        "I'm not set up here yet. I won't be able to help much, so you should probably ask an admin for help.",
        "Yeah… this server isn't registered yet. That's probably why things aren't working."
    ];
    
    // Edge Case: A server with only bots?
    public static readonly string[] NoUsersToRegister =
    [
        "Uh… there's no one here to register. That's kinda awkward.",
        "I looked around, but there aren't any real users to add yet.",
        "So, funny thing, there's nobody here for me to register."
    ];

    // --- WELCOME CONFIG ---
    
    // Success for /enanan welcome
    // {0} = Channel ID
    public static readonly string[] WelcomeChannelSet =
    [
        "Got it! I'll be welcoming new folks in <#{0}> from now on.",
        "Alright, welcome messages are going to <#{0}> now.",
        "Done! I've set <#{0}> as the welcome channel."
    ];
}