using EnananBot.Messages;
using EnananBot.Objects.Enums;

namespace EnananBot.Services;

/// <summary>
/// A central service for generating bot response strings. 
/// It randomly selects messages from predefined arrays to give the bot a varied "personality" 
/// and appends the context-appropriate emoji.
/// </summary>
public sealed class MessageService
{
    private static readonly Random Rng = Random.Shared;
    
    // --- Role Management Responses ---
    
    public string RoleCreateSuccess(params object[] args)
        => Build(RoleMessages.CreateSuccess, EmojiCategory.Success, args);

    public string RoleCreateError(params object[] args)
        => Build(RoleMessages.CreateError, EmojiCategory.Error, args);

    public string RoleEditSuccess(params object[] args)
        => Build(RoleMessages.EditSuccess, EmojiCategory.Success, args);

    public string RoleEditError(params object[] args)
        => Build(RoleMessages.EditError, EmojiCategory.Error, args);

    public string RoleDeleteSuccess(params object[] args)
        => Build(RoleMessages.DeleteSuccess, EmojiCategory.Success, args);

    public string RoleDeleteError(params object[] args)
        => Build(RoleMessages.DeleteError, EmojiCategory.Error, args);
    
    public string RoleEditNoChanges(params object[] args)
        => Build(RoleMessages.RoleEditNoChanges, EmojiCategory.Failure, args);
    
    // --- Validation Responses ---
    
    public string ValidationEmptyName(params object[] args)
        => Build(ValidationMessages.EmptyName, EmojiCategory.Failure, args);

    public string ValidationEmptyColor(params object[] args)
        => Build(ValidationMessages.EmptyColor, EmojiCategory.Failure, args);

    public string ValidationInvalidColor(params object[] args)
        => Build(ValidationMessages.InvalidColor, EmojiCategory.Failure, args);

    public string ValidationUserHasRole(params object[] args)
        => Build(ValidationMessages.UserHasRole, EmojiCategory.Failure, args);

    public string ValidationUserDoesNotHaveRole(params object[] args)
        => Build(ValidationMessages.UserDoesNotHaveRole, EmojiCategory.Failure, args);

    public string ValidationRoleAmountIsAtMax(params object[] args)
        => Build(ValidationMessages.RoleAmountIsAtMax, EmojiCategory.Failure, args);

    public string ValidationAllInputsAreEmpty(params object[] args)
        => Build(ValidationMessages.AllInputsEmpty, EmojiCategory.Failure, args);
    
    // --- System & Setup Responses ---

    public string SystemError()
        => Build(SystemMessages.GenericError, EmojiCategory.Error);

    public string ServerSetupSuccess(params object[] args)
        => Build(SystemMessages.ServerSetupSuccess, EmojiCategory.Success, args);

    public string ServerSetupError(params object[] args)
        => Build(SystemMessages.ServerSetupError, EmojiCategory.Error, args);
    
    public string NoUsersToRegister() =>
        Build(SystemMessages.NoUsersToRegister, EmojiCategory.Error);

    public string UserListHeader(params object[] args) =>
        Build(MiscMessages.UserListHeader, EmojiCategory.Misc, args);

    public string WelcomeChannelSet(params object[] args) =>
        Build(SystemMessages.WelcomeChannelSet, EmojiCategory.Success, args);

    public string MessageSentToDMs()
        => Build(SystemMessages.MessageSentToDMs, EmojiCategory.Misc);

    public string AdminDMsAreClosed()
        => Build(SystemMessages.AdminDMsAreClosed, EmojiCategory.Failure);

    public string ServerIsAlreadyRegistered()
        => Build(SystemMessages.ServerIsAlreadyRegistered, EmojiCategory.Misc);

    public string ServerIsNotRegistered()
        => Build(SystemMessages.ServerIsNotRegistered, EmojiCategory.Failure);
    
    // --- Image Generation & Fun Responses ---

    public string MessagePreviewSuccess(params object[] args)
        => Build(ColorMessages.MessagePreviewSuccess, EmojiCategory.Image, args);

    public string MessagePreviewError(params object[] args)
        => Build(ColorMessages.MessagePreviewError, EmojiCategory.Image, args);

    public string ColorPaletteSuccess(params object[] args)
        => Build(ColorMessages.ColorPaletteSuccess, EmojiCategory.Image, args);

    public string ColorPaletteError()
        => Build(ColorMessages.ColorPaletteError, EmojiCategory.Image);

    public string ListAllColors(params object[] args)
        => Build(ColorMessages.ListAllColors, EmojiCategory.Misc, args);

    public string Welcome(params object[] args)
        => Build(MiscMessages.WelcomeMessages, EmojiCategory.Misc, args);

    public string Credits()
        => Build(MiscMessages.CreditsMessages, EmojiCategory.Misc);

    public string Donate(params object[] args)
        => Build(MiscMessages.DonationMessages, EmojiCategory.Misc, args);
    
    public string Invite(params object[] args)
        => Build(MiscMessages.InviteLink, EmojiCategory.Misc, args);
    
    /// <summary>
    /// Core builder method. Picks a random template, formats it, and adds an emoji.
    /// </summary>
    private static string Build(string[] messages, EmojiCategory category, params object[] args)
    {
        if (messages.Length == 0)
            return string.Empty;

        // Pick a random variation from the array
        var message = messages[Rng.Next(messages.Length)];
        
        var formatted = SafeFormat(message, args);
        var emoji = EmojiService.Pick(category);

        return $"{formatted} {emoji}";
    }

    /// <summary>
    /// Wrapper for string.Format that swallows FormatExceptions.
    /// This prevents the bot from crashing if a message template has {0} but no args were passed.
    /// </summary>
    private static string SafeFormat(string message, params object[] args)
    {
        if (args.Length == 0) return message;

        try
        {
            return string.Format(message, args);
        }
        catch (FormatException)
        {
            // If formatting fails, fallback to the raw message rather than crashing the request
            return message;
        }
    }
}