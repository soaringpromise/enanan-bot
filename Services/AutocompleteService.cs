using EnananBot.Objects.Enums;
using EnananBot.Utils;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace EnananBot.Services;

/// <summary>
/// Container for Autocomplete Logic implementations.
/// Allows slash commands to provide dynamic suggestions as the user types.
/// </summary>
public static class AutocompleteService
{
    /// <summary>
    /// Provides autocomplete suggestions for role name decorators.
    /// Usage: [Autocomplete(typeof(AutocompleteService.Decorators))] in the command parameter.
    /// </summary>
    public class DecoratorsProvider : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(
            ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            // Get what the user has typed so far (or empty string if they just clicked the field)
            var input = option.Value ?? string.Empty;

            // Filter the available decorators based on the user's input.
            // Case-insensitive search
            var matches = Objects.Decorators.All.Where(x =>
                x.Key.Contains(input, StringComparison.InvariantCultureIgnoreCase));

            // Map valid matches to Discord Choice objects (Name = Display, Value = ID/Key)
            var choices = matches
                .Select(x => new ApplicationCommandOptionChoiceProperties(x.Key, x.Value))
                // Discord allows a maximum of 25 autocomplete suggestions
                // Exceeding this will cause the interaction to fail
                .Take(25);

            return new ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?>(choices);
        }
    }

    public class NamedColorsProvider : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(
            ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            var input = option.Value ?? string.Empty;

            var matches = typeof(NamedColors)
                .GetEnumNames()
                .Where(x => x.StartsWith(input, StringComparison.InvariantCultureIgnoreCase))
                .Take(25)
                .Select(x => new ApplicationCommandOptionChoiceProperties(x.FormatEnumName(), x));

            return new ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?>(matches);
        }
    }
}