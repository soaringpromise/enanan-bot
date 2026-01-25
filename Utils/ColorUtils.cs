using ColorMine.ColorSpaces;
using NetCord;

namespace EnananBot.Utils;

/// <summary>
/// A utility class for complex color manipulation.
/// Handles parsing user inputs (Hex, Names), calculating contrast ratios,
/// and generating color palettes using the perceptual Lch color space.
/// </summary>
public static class ColorUtils
{
    // A safe default color (Dark Gray) to use if parsing fails completely
    private static readonly Color FallbackGray = new(67, 67, 67);

    /// <summary>
    /// The main entry point for converting user input into a NetCord Color object.
    /// Handles Hex codes (#FF0000), shorthand (#F00), and Names (Red).
    /// </summary>
    public static Color GetDiscordColor(string colorString)
    {
        var normalized = NormalizeColorString(colorString);

        if (normalized == null)
            return FallbackGray;

        // Convert Hex string to UInt32, then extract bytes for R, G, B
        var hexVal = Convert.ToUInt32(normalized, 16);

        return new Color(
            (byte)((hexVal >> 16) & 0xFF), // Red
            (byte)((hexVal >> 8) & 0xFF),  // Green
            (byte)(hexVal & 0xFF)          // Blue
            );
    }
    
    /// <summary>
    /// Cleans and validates a raw color string.
    /// Returns a clean 6-digit hex string (e.g., "ff0000") or null if invalid.
    /// </summary>
    public static string? NormalizeColorString(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim().ToLowerInvariant();
        
        // Strip optional hashtag
        if (input.StartsWith('#'))
            input = input[1..];
        
        // Convert 'fff' to 'ffffff'
        input = ExpandCssShorthandRgb(input);
        
        // Check if it's a known name like "Red", "Cyan", "CornflowerBlue"
        var named = System.Drawing.Color.FromName(input);
        if (named.IsKnownColor)
            return $"{named.R:X2}{named.G:X2}{named.B:X2}".ToLowerInvariant();

        return !IsValidHexColorNoAlpha(input) ? null : input;
    }
    
    // Expands 3-digit hex (Web style) to 6-digit hex
    private static string ExpandCssShorthandRgb(string input)
    {
        if (input.Length != 3 || !input.All(Uri.IsHexDigit))
            return input;

        // "a1f" => "aa11ff"
        return string.Concat(input.Select(c => $"{c}{c}"));
    }
    
    // Validates that the string contains only 3 or 6 valid hex characters
    private static bool IsValidHexColorNoAlpha(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        var start = value[0] == '#' ? 1 : 0;
        var length = value.Length - start;

        if (length != 3 && length != 6)
            return false;

        for (var i = start; i < start + length; i++)
            if (!Uri.IsHexDigit(value[i]))
                return false;

        return true;
    }

    /// <summary>
    /// Wrapper to get a formatted "#RRGGBB" string for display.
    /// </summary>
    public static string GetNormalizedHex(string colorString)
    {
        var normalized = NormalizeColorString(colorString);
        return normalized == null ? "#434343" : $"#{normalized}";
    }
    
    /// <summary>
    /// Calculates whether Black or White text is more readable on top of the given background color.
    /// Uses standard W3C relative luminance formula.
    /// </summary>
    public static string GetContrastColor(string hexBg)
    {
        var c = GetDiscordColor(hexBg);

        var r = Channel(c.Red);
        var g = Channel(c.Green);
        var b = Channel(c.Blue);

        // Calculate Luminance
        var lum = 0.2126 * r + 0.7152 * g + 0.0722 * b;

        // Threshold of 0.5 determines if the color is "light" or "dark"
        return lum > 0.5 ? "#000000" : "#FFFFFF";

        // Helper to linearize gamma-corrected RGB values
        static double Channel(double v)
        {
            v /= 255.0;
            return v <= 0.03928
                ? v / 12.92
                : Math.Pow((v + 0.055) / 1.055, 2.4);
        }
    }
    
    /// <summary>
    /// Generates a gradient palette based on a single input color.
    /// Uses Lch color space to ensure the gradient looks natural and vibrant.
    /// </summary>
    public static string[] GetPalette(string colorString, int steps = 9)
    {
        if (steps < 2) steps = 2;

        var baseColor = GetDiscordColor(colorString);

        // Convert RGB -> Lch (Lightness, Chroma, Hue)
        var baseRgb = new Rgb { R = baseColor.Red, G = baseColor.Green, B = baseColor.Blue };
        var baseLab = baseRgb.To<Lab>();
        var baseLch = baseLab.To<Lch>();

        var darkL = Math.Max(0, baseLch.L - 40);
        var lightL = Math.Min(100, baseLch.L + 40);

        // Generate a Darker variant (lower Lightness)
        var darkLch = new Lch
        {
            L = darkL,
            C = ScaleChroma(darkL, baseLch.C),
            H = baseLch.H
        };

        // Generate a Lighter variant (higher Lightness)
        var lightLch = new Lch
        {
            L = lightL,
            C = ScaleChroma(lightL, baseLch.C),
            H = baseLch.H
        };

        var results = new string[steps];

        // Create the gradient
        for (var i = 0; i < steps; i++)
        {
            var t = (double)i / (steps - 1);

            // Interpolate: 
            // First half: Dark -> Base
            // Second half: Base -> Light
            // This ensures the user's chosen color is exactly in the center
            var resultLch = t < 0.5
                ? InterpolateLch(darkLch, baseLch, t * 2)
                : InterpolateLch(baseLch, lightLch, (t - 0.5) * 2);

            var safeRgb = ToSafeRgb(resultLch);

            results[i] = $"#{Clamp(safeRgb.R):X2}{Clamp(safeRgb.G):X2}{Clamp(safeRgb.B):X2}";
        }

        return results;
    }
    
    // Linear interpolation between two Lch colors
    private static Lch InterpolateLch(Lch c1, Lch c2, double t) => new()
    {
        L = c1.L + (c2.L - c1.L) * t,
        C = c1.C + (c2.C - c1.C) * t,
        H = InterpHue(c1.H, c2.H, t)
    };
    
    private static double InterpHue(double h1, double h2, double t)
    {
        var delta = (h2 - h1 + 540) % 360 - 180;
        return (h1 + delta * t + 360) % 360;
    }

    private static double ScaleChroma(double luminance, double baseChroma)
    {
        // Tapers chroma near white and black
        var factor = Math.Sin(luminance / 100 * Math.PI);
        return baseChroma * factor;
    }

    private static Rgb ToSafeRgb(Lch lch)
    {
        var c = lch.C;

        while (c > 0)
        {
            var rgb = new Lch { L = lch.L, C = c, H = lch.H }.To<Rgb>();

            if (rgb.R is >= 0 and <= 255 &&
                rgb.G is >= 0 and <= 255 &&
                rgb.B is >= 0 and <= 255)
                return rgb;

            c -= 1;
        }

        return new Rgb { R = 67, G = 67, B = 67 };
    }

    private static byte Clamp(double v) => (byte)Math.Max(0, Math.Min(255, v));
}