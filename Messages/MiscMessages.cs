namespace EnananBot.Messages;

/// <summary>
/// A static container for miscellaneous dialogue lines.
/// Includes: Credits, Donations, Welcome events, Admin headers, and Link Fixer responses.
/// </summary>
public static class MiscMessages
{
    // Dialogue for /credits
    public static readonly string[] CreditsMessages =
    [
        "These kinds of things don't just happen overnight! Lots of people have to work on a project like this for it to work. I mean just LOOK at all these names!",
        "It's not just about talent, you know? It takes real effort and long nights to create something from nothing. These are the people who actually put in the work to make this bot a reality. You better appreciate it.",
        "Seriously, take a moment to look through these. Every name here represents time, effort, and way too many late nights spent making sure this thing actually works. That kind of dedication deserves some respect."
    ];
    
    // Dialogue for /donate
    // {0} = The Ko-Fi link
    public static readonly string[] DonationMessages =
    [
        "If you like my work, you can support me here! {0}",
        "Art isn't free, freelancers need financial support! You can donate here: {0}",
        "Anyway… if you wanna help keep things running, here's the link: {0}"
    ];

    // Dialogue for /invite
    // {0} = The invite link
    public static readonly string[] InviteLink =
    [
        "Oh… you want to invite me somewhere else? I guess that's kinda nice. Here:\n**[Invite Enanan to your server!](<{0}>)**",
        "Another server, huh? …Alright. I'll go.\n**[Invite Enanan to your server!](<{0}>)**",
        "Well… if you think I'll be useful, then fine. Click here to invite me:\n**[Invite Enanan to your server!](<{0}>)**",
        "I mean, I *am* pretty helpful, so… yeah. You can invite me with this:\n**[Invite Enanan to your server!](<{0}>)**",
        "Guess I can handle one more place. Just don't expect miracles.\n**[Invite Enanan to your server!](<{0}>)**",
        "…Thanks for wanting to bring me along. Here's the invite link:\n**[Invite Enanan to your server!](<{0}>)**"
    ];
    
    // Dialogue for GuildJoinEvent (when a user joins the server).
    // <@{0}> is the raw Discord syntax for Mentioning a user by ID.
    public static readonly string[] WelcomeMessages =
    [
        "Oh— hey. Didn't expect company, <@{0}>.",
        "Huh. New face, huh? Welcome, <@{0}>.",
        "Alright, alright… welcome in, <@{0}>.",
        "So you're the new one everyone was talking about, <@{0}>?",
        "Welcome, <@{0}>. Try not to break anything, okay?",
        "Oh wow, someone actually joined. Hey there, <@{0}>.",
        "Guess I should say welcome, huh? Hi, <@{0}>.",
        "New arrival detected… yeah, yeah. Welcome, <@{0}>.",
        "Hey. Yeah, you. <@{0}>. Welcome.",
        "Alright, make yourself comfortable, <@{0}>. Or don't. Up to you."
    ];
    
    // Header for /enanan list
    // {0} = Server Name
    public static readonly string[] UserListHeader =
    [
        "**Here's everyone I've got registered in {0}:**",
        "**Alright, here's the full user list for {0}:**",
        "**These are all the users I'm tracking in {0}:**"
    ];
    
    // Dialogue for the Automatic Link Fixer (e.g., Twitter -> FxTwitter).
    // Complex formatting:
    // {0} = Original Platform Name (e.g. "Twitter")
    // {1} = Original URL
    // {2} = Fixed Platform Name (e.g. "FxTwitter")
    // {3} = Fixed URL
    public static readonly string[] LinkFixed =
    [
        "Oh, I fixed your original **[{0}](<{1}>)** link using **[{2}]({3})**.",
        "I took a look at that **[{0}](<{1}>)** link and updated it to **[{2}]({3})**.",
        "Your **[{0}](<{1}>)** link has been corrected to **[{2}]({3})**~!",
        "I went ahead and fixed that **[{0}](<{1}>)** link using **[{2}]({3})**.",
        "Alright! **[{0}](<{1}>)** should now point to **[{2}]({3})**.",
        "The **[{0}](<{1}>)** link is now updated with **[{2}]({3})**, hehe.",
        "I've fixed that **[{0}](<{1}>)** link to redirect properly, here: **[{2}]({3})**.",
        "Your original **[{0}](<{1}>)** link has been replaced with **[{2}]({3})**.",
        "I checked **[{0}](<{1}>)** and fixed it to **[{2}]({3})**, nice, isn't it?.",
        "Here you go: **[{0}](<{1}>)** is now corrected as **[{2}]({3})**."
    ];

    // Same as above, but used when the user originally wrapped their link in spoilers ||...||
    // The bot respects the spoiler tag.
    public static readonly string[] LinkSpoilered =
    [
        "Oh! I wrapped your fixed **[{0}](<{1}>)** link with spoilers using **[{2}]({3})**!",
        "Peek-a-boo! Your **[{0}](<{1}>)** link is now spoilered and fixed: **[{2}]({3})**.",
        "I went ahead and fixed that **[{0}](<{1}>)** link, now it's spoilered: **[{2}]({3})**.",
        "Alright, **[{0}](<{1}>)** is now corrected and wrapped in spoilers using **[{2}]({3})**.",
        "Your link **[{0}](<{1}>)** is now both fixed and spoilered: **[{2}]({3})**~!",
        "I fixed **[{0}](<{1}>)** for you and added spoilers: **[{2}]({3})**.",
        "Here's your **[{0}](<{1}>)** link, properly fixed and spoilered: **[{2}]({3})**.",
        "Fix complete! **[{0}](<{1}>)** is now spoilered and redirects to **[{2}]({3})**."
    ];
}