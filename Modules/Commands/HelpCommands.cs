using EnananBot.Utils;
using NetCord.Services.ApplicationCommands;

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace EnananBot.Modules.Commands;

/// <summary>
/// Slash Command Module for Documentation.
/// Base Command: /help
/// Provides detailed usage instructions for all other modules.
/// Inherits directly from "ApplicationCommandModule" to be accessible even in unregistered guilds.
/// </summary>
[SlashCommand("help", "Shows help for commands.")]
public class HelpCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    /// <summary>
    /// Command: /help all
    /// A high-level overview of available command groups.
    /// </summary>
    [SubSlashCommand("all", "Lists all available command groups.")]
    public async Task AllCommands()
    {
        // Using C# 11 Raw String Literals (""") for clean multi-line text without messy \n characters.
        const string message =
            """
            **Available command groups:**
            `/role [create | edit | delete]`
            `/color [preview | palette | list]`
            `/management [setup | list]`
            `/misc [credits | help]`
            
            Use `/help <group>` to see detailed info for each group.
            """;

        await ResponseUtils.SendSimpleResponse(Context, message);
    }
    
    /// <summary>
    /// Command: /help role
    /// Explains how to create and manage personal custom roles.
    /// </summary>
    [SubSlashCommand("role", "Detailed help for role commands.")]
    public async Task RoleCommands()
    {
        const string message =
            """
            **Role Commands**
            `/role create [role name] [color name/hex] (decorator)`
            Create a unique role for yourself, one per server.
                
            `/role edit (role name) (color name/hex) (decorator)`
            Edit your personal role's name, color or decorator. You need a unique role first.
                
            `/role delete`
            Delete your personal role from this server. You need a unique role first.
            """;

        await ResponseUtils.SendSimpleResponse(Context, message);
    }
    
    /// <summary>
    /// Command: /help color
    /// Explains the color visualization tools.
    /// </summary>
    [SubSlashCommand("color", "Detailed help for color commands.")]
    public async Task ColorCommands()
    {
        const string message =
            """
            **Color Commands**
            `/color preview [color name/hex]`
            Generate a message preview with the given color.
            
            `/color palette [color name/hex]`
            Generate a 3x3 color palette with the given color.
            
            `/color list`
            List all 148 named colors compatible with this bot.
            """;

        await ResponseUtils.SendSimpleResponse(Context, message);
    }
    
    /// <summary>
    /// Command: /help management
    /// Explains admin/setup commands.
    /// </summary>
    [SubSlashCommand("management", "Detailed help for management commands.")]
    public async Task ManagementCommands()
    {
        const string message =
            """
            **Management Commands**
            `/enanan setup`
            Initial manual setup if automatic registration fails.
            
            `/enanan list`
            Lists all users in the guild with their roles, sent to DM.
            
            `/enanan welcome [channel]`
            Sets up or updates the welcome channel for the server.
            """;

        await ResponseUtils.SendSimpleResponse(Context, message);
    }
    
    /// <summary>
    /// Command: /help misc
    /// Credits and meta-info.
    /// </summary>
    [SubSlashCommand("misc", "Detailed help for miscellaneous commands.")]
    public async Task MiscCommands()
    {
        const string message =
            """
            **Miscellaneous Commands**
            `/credits`
            Shows the contributors to this bot.
            
            `/help [all | role | color | enanan]`
            Lists all command groups.
            
            `/donate`
            Shows a Ko-Fi link for donations.
            
            `/invite`
            Gives the permanent invite link for the bot.
            """;

        await ResponseUtils.SendSimpleResponse(Context, message);
    }
}