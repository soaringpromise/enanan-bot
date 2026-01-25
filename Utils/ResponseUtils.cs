using System.Collections.Concurrent;
using EnananBot.Embeds;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace EnananBot.Utils;

/// <summary>
/// A helper class to manage Discord Interaction responses.
/// Automatically handles the distinction between the "Initial Response" (which uses the Interaction Token)
/// and "Follow-up Messages" (which require a webhook call), preventing "Interaction already acknowledged" errors.
/// </summary>
public static class ResponseUtils
{
    // A thread-safe dictionary to track which interaction IDs have already been responded to.
    // Key = Interaction ID (ulong). Value = dummy byte (we just need the key).
    private static readonly ConcurrentDictionary<ulong, byte> RespondedInteractions = new();
    
    /// <summary>
    /// The core logic engine. Checks if this interaction hasn't been replied to yet
    /// and dispatches the message via the correct API method.
    /// </summary>
    private static async Task SendAsync(
        ApplicationCommandContext context,
        InteractionMessageProperties message,
        bool ephemeral)
    {
        var interactionId = context.Interaction.Id;

        // Default to suppressing push notifications (silent message) unless specified otherwise
        var flags = MessageFlags.SuppressNotifications;
        
        // Combine flags using bitwise OR if ephemeral is requested (only visible to the user)
        if (ephemeral) flags |= MessageFlags.Ephemeral;

        message.WithFlags(flags);
        
        // TryAdd returns true if the key did NOT exist (meaning this is the first response)
        // It returns false if the key already exists (meaning we already responded).
        var isFirstResponse = RespondedInteractions.TryAdd(interactionId, 0);

        if (isFirstResponse)
        {
            // First time: Use the official Interaction Response
            await context.Interaction.SendResponseAsync(
                InteractionCallback.Message(message)
            );
        }
        else
        {
            // Subsequent times: Send a follow-up message
            await context.Interaction.SendFollowupMessageAsync(message);
        }
    }

    /// <summary>
    /// Explicitly defers an interaction (shows "EnananBot is thinking...").
    /// This counts as the "First Response", so later messages will be follow-ups.
    /// </summary>
    public static async Task DeferAsync(ApplicationCommandContext context, bool ephemeral = false)
    {
        var flags = MessageFlags.SuppressNotifications;
        if (ephemeral) flags |= MessageFlags.Ephemeral;
        
        // Mark as responded so SendAsync knows to use Followups later.
        RespondedInteractions.TryAdd(context.Interaction.Id, 0);

        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(flags)
        );
    }
    
    /// <summary>
    /// Wraps a simple text string into an Embed and sends it.
    /// </summary>
    public static Task SendSimpleResponse(ApplicationCommandContext context, string message, bool isEphemeral = false)
    {
        var embed = EmbedBuilder.SimpleMessageEmbed(message);

        var props = new InteractionMessageProperties().AddEmbeds(embed);
        
        return SendAsync(context, props, isEphemeral);
    }

    /// <summary>
    /// Sends a message with an attached image file (Stream).
    /// Used primarily for role previews and color swatches.
    /// </summary>
    public static async Task SendImageResponse(
        ApplicationCommandContext context,
        string message,
        Stream imageStream,
        bool isEphemeral = false)
    {
        const string fileName = "preview.webp";

        // Reset stream position to the beginning if it was previously read.
        if (imageStream.CanSeek && imageStream.Position != 0)
            imageStream.Position = 0;

        // Prepare the file attachment.
        var attachment = new AttachmentProperties(fileName, imageStream);

        // Reference the attachment inside the embed using the "attachment://" protocol.
        var embed =
            EmbedBuilder.ImageEmbed($"attachment://{fileName}", message);

        var props = new InteractionMessageProperties()
            .AddEmbeds(embed)
            .AddAttachments(attachment);

        await SendAsync(context, props, isEphemeral);
    }

    /// <summary>
    /// Sends a complex embed with multiple fields (Title + Key/Value pairs).
    /// Used for Info displays.
    /// </summary>
    public static Task SendFieldResponse(
        ApplicationCommandContext context, string message, 
        IEnumerable<(string Name, string Value, bool Inline)> fields, bool isEphemeral = false)
    {
        var embed = EmbedBuilder.FieldEmbed(message, fields);

        return SendAsync(
            context,
            new InteractionMessageProperties().AddEmbeds(embed),
            isEphemeral
        );
    }
    
    /// <summary>
    /// Cleans up the tracking dictionary.
    /// IMPORTANT: This should be called after a command is fully finished to prevent memory leaks.
    /// </summary>
    public static void Clear(ApplicationCommandContext context)
    {
        RespondedInteractions.TryRemove(context.Interaction.Id, out _);
    }
}