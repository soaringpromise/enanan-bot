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
}