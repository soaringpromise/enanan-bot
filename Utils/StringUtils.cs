using System.Text;
using System.Text.RegularExpressions;

namespace EnananBot.Utils;

/// <summary>
/// A static utility class providing extension methods for string manipulation.
/// Handles tasks like array formatting, role name decoration, and text sanitization.
/// </summary>
public static class StringUtils
{
    // A compiled Regex used to strip out special characters, keeping only lowercase alphanumeric chars.
    // Used for sanitizing user inputs (likely for database keys or strict comparisons).
    private static readonly Regex AllowedChars =
        new("[^#a-z0-9]",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.NonBacktracking);
    
    private static readonly Random Random = new();

    /// <summary>
    /// Formats an array of strings into a neat, comma-separated list inside a code block.
    /// Breaks lines every 3 items to prevent horizontal scrolling in Discord.
    /// </summary>
    public static string PrettifyArray(this string[] array)
    {
        if (array.Length == 0) return string.Empty;

        // Pre-allocate memory to avoid resizing the buffer repeatedly
        var sb = new StringBuilder(array.Length * 12);

        sb.Append("```"); // Start a Markdown code block

        for (var i = 0; i < array.Length; i++)
        {
            // Add a newline only before the very first item to separate it from the backticks
            if (i == 0)
                sb.AppendLine();
        
            sb.Append(array[i]);

            var isEndOfLine = (i + 1) % 3 == 0;
            var isLast = i == array.Length - 1;

            // Logic to determine if we need a newline (every 3 items) or just a comma
            if (isEndOfLine && !isLast)
                sb.AppendLine();
            else if (!isLast)
                sb.Append(", ");
        }

        sb.AppendLine();
        sb.Append("```"); // End Markdown code block

        return sb.ToString();
    }

    extension(string input)
    {
        /// <summary>
        /// Wraps a role name in decorative characters (e.g., "★ Role Name ★").
        /// Handles both single-char decorators (star on both sides) and pairs (open/close brackets).
        /// </summary>
        public string DecorateRoleName(string decorator)
        {
            if (string.IsNullOrWhiteSpace(decorator)) return input;
        
            // EnumerateRunes is necessary to correctly handle complex Unicode characters (emojis)
            var enumerator = decorator.EnumerateRunes();

            if (!enumerator.MoveNext()) return input;

            var first = enumerator.Current;

            // If the decorator is only 1 char (e.g., "★"), use it on both sides
            if (!enumerator.MoveNext()) return $"{first} {input} {first}";

            // If the decorator has 2 chars (e.g., "[]"), use first as prefix and second as suffix
            var second = enumerator.Current;

            return $"{first} {input} {second}";
        }

        /// <summary>
        /// Collapses multiple spaces into a single space and trims the ends.
        /// "  My   Role  " -> "My Role"
        /// </summary>
        public string NormalizeSpaces()
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var sb = new StringBuilder(input.Length);
            var previousWasSpace = false;

            foreach (var c in input)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (previousWasSpace) continue; // Skip if we just saw a space

                    sb.Append(' ');
                    previousWasSpace = true;
                }
                else
                {
                    sb.Append(c);
                    previousWasSpace = false;
                }
            }
            return sb.ToString().Trim();
        }

        /// <summary>
        /// Appends fake social media statistics (Views, Likes, RTs) to the footer of a message.
        /// </summary>
        public string FakeStats()
        {
            // Generate plausible random numbers for the stats
            var comments = Random.Next(10, 101);
            var likes = Random.Next(Math.Max(comments, 1), 10001); // Likes usually > comments
            var shares = Random.Next(10, likes + 1);                // Shares usually < likes
            var views = Random.Next(Math.Max(likes, 1), 1_000_001); // Views usually > likes

            var separator =
                string.IsNullOrWhiteSpace(input) || input.TrimEnd().EndsWith("```")
                    ? string.Empty
                    : "\n\n";
            
            // Format: "Original Text \n\n 💬 50 🔁 1.2K ❤️ 5K 👁️ 1M"
            return 
                $"{input}{separator}💬 **{comments}** 🔁 **{FormatNumber(shares)}** ❤️ **{FormatNumber(likes)}** 👁️ **{FormatNumber(views)}**";

            // Local helper to format numbers like "1.5K" or "2M"
            string FormatNumber(int n) =>
                n switch
                {
                    >= 1_000_000 => (n / 1_000_000d).ToString("0.#") + "M",
                    >= 1_000 => (n / 1_000d).ToString("0.#") + "K",
                    _ => n.ToString()
                };
        }

        /// <summary>
        /// Removes all characters except lowercase letters, numbers, and '#'.
        /// </summary>
        public string StripInvalidChars()
        {
            return string.IsNullOrEmpty(input)
                ? string.Empty
                : AllowedChars.Replace(input, string.Empty);
        }

        public string FormatEnumName()
        {
            var sb = new StringBuilder();
            sb.Append(input[0]); // Append the first character as-is

            for (var i = 1; i < input.Length; i++)
            {
                var c = input[i];
                if (char.IsUpper(c)) sb.Append(' '); // Add space before an uppercase letter
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}