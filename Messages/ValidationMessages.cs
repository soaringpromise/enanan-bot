namespace EnananBot.Messages;

/// <summary>
/// A static container for Validation failure dialogue.
/// Used by the ValidationService when business rules are violated.
/// </summary>
public static class ValidationMessages
{
    // Error: User provided an empty string for the role name
    // {0} = Username
    public static readonly string[] EmptyName =
    [
        "Hello? You can't just not have a role name, **{0}**.",
        "What the heck? Why's your role name blank?",
        "I need *something* to call it, you know, **{0}**."
    ];

    // Error: User provided an empty, whitespace color
    // {0} = Username
    public static readonly string[] EmptyColor =
    [
        "Come on! You've gotta add some color first!",
        "Uh, where's the color, **{0}**?",
        "You didn't forget the color part, did you?"
    ];
    
    // Error: User input "blurple" or "0xZZZZZZ"
    // {0} = Username
    public static readonly string[] InvalidColor =
    [
        "What the… That's not a real color, is it?",
        "Wait a second, **{0}**, that color doesn't exist!",
        "Hey! What are you trying to pull?!",
        "Hey, **{0}**. Don't go trying pulling pranks on me.",
    ];

    // Error: User tries '/role create' but they already have a custom role
    // {0} = Username
    public static readonly string[] UserHasRole =
    [
        "Hey, looks like you've already assigned yourself a role, **{0}**.",
        "You've already got that one, I can literally see it on your profile.",
        "Yeah… you already have a role, **{0}**."
    ];

    // Error: User tries '/role edit' or '/role delete' but they have no custom role
    // {0} = Username
    public static readonly string[] UserDoesNotHaveRole =
    [
        "I don't see you on the role list, **{0}**…",
        "Doesn't look like you've got any special roles yet.",
        "Nope. I don't think you have a role yet, **{0}**."
    ];

    // Error: The Discord Server has reached the 250-Role Limit
    // {0} = Username
    public static readonly string[] RoleAmountIsAtMax =
    [
        "Holy, how **many** roles do you have here?! Clean up that list, sheesh!",
        "Uh… sorry, **{0}**, but you've already got too many roles in your server.",
        "You might wanna delete a *few* roles first, **{0}**. I'm kinda at my limit here."
    ];
    
    // Error: User provided no role name, no color, and no decorator on '/role edit'
    // {0} = Username
    public static readonly string[] AllInputsEmpty =
    [
        "…So you just hit enter with nothing filled in, **{0}**?",
        "Wow. Zero inputs. Truly impressive, **{0}**.",
        "You could've at least typed *something*, you know.",
        "I can't work with literally nothing, **{0}**. Try again.",
        "Uh… did you mean to leave *everything* blank?",
        "Are you trying to waste my time, **{0}**, or…?",
        "At least give me one thing to change, **{0}**.",
        "I need *some* kind of input to work with."
    ];
}