using System.Text.RegularExpressions;

namespace EnananBot.Services;

/// <summary>
/// A utility service that compiles dynamic Route definitions into efficient .NET Regular Expressions.
/// Used to detect and parse URLs (like Twitter/X posts) from user messages.
/// </summary>
public sealed class RegexGeneratorService
{
    /// <summary>
    /// Batch generates regex patterns for multiple route definitions against a set of domains.
    /// </summary>
    /// <param name="domainNames">A list of domains to match (e.g., "twitter.com", "x.com").</param>
    /// <param name="routes">A dictionary where Key = RoutePattern and Value = Optional Query Parameters to look for.</param>
    public static Dictionary<string, Regex> GenerateRoutes(
        IEnumerable<string> domainNames,
        Dictionary<string, string[]?> routes)
    {
        // Materialize the list once to avoid multiple enumerations
        var domainList = domainNames as IReadOnlyCollection<string>
                         ?? domainNames.ToArray();

        var result = new Dictionary<string, Regex>();

        foreach (var route in routes)
        {
            result[route.Key] = GenerateRegex(domainList, route.Key, route.Value);
        }
        return result;
    }

    /// <summary>
    /// Compiles a single Regex object capable of matching a specific URL route across multiple domains.
    /// Supports "Express.js" style parameters (e.g., /:id) and converts them to Regex Named Groups.
    /// </summary>
    private static Regex GenerateRegex(
        IEnumerable<string> domainNames,
        string route,
        string[]? parameters = null)
    {
        // Ensure the route starts with a slash for consistent path matching
        if (!route.StartsWith('/')) route = "/" + route;

        // Create a non-capturing group for domains: (twitter.com|x.com)
        var escapedDomains = domainNames.Select(Regex.Escape);
        var domainRegex = "(?<domain>" + string.Join("|", escapedDomains) + ")";

        var routeRegex = route;

        // --- Route Parameter Transformation Logic ---
        // The following blocks replace simple parameter placeholders with complex Regex groups

        // 1. Optional param with constraint: /:id(\d+)? -> (?:/(\d+))?
        routeRegex = Regex.Replace(
            routeRegex,
            @"/:(\w+)\(([^/]+)\)\?",
            "(?:/(?:$2))?");

        // 2. Optional simple param: /:id? -> (?:/[^/?#]+)?
        routeRegex = Regex.Replace(
            routeRegex,
            @"/:(\w+)\?",
            "(?:/[^/?#]+)?");

        // 3. Required param with constraint: /:id(\d+) -> (?<id>\d+)
        routeRegex = Regex.Replace(
            routeRegex,
            @"([^?]):(\w+)\(([^/]+)\)",
            "$1(?<$2>$3)");

        // 4. Required simple param: /:id -> (?<id>[^/?#]+)
        routeRegex = Regex.Replace(
            routeRegex,
            @"([^?]):(\w+)",
            "$1(?<$2>[^/?#]+)");

        // --- Query String Logic ---
        // Uses Positive Lookaheads (?=...) to find specific query parameters regardless of their order in the URL
        var queryStringParamRegexes = new List<string>();
        if (parameters != null)
        {
            queryStringParamRegexes.AddRange(
                parameters.Select(param =>
                    $"(?:(?=(?:\\?|.*&){param}=(?<{param}>[^&#]+)))?"));
        }
        
        // Combine all query string lookaheads
        var queryStringRegex =
            "(?:\\?(?:" + string.Join("&", queryStringParamRegexes) + ")?)?";

        // --- Final Assembly ---
        // ^                 : Start of string
        // https?://         : Protocol
        // (?:...)?          : Optional subdomain
        // domainRegex       : The list of valid domains
        // routeRegex        : The path pattern
        // queryStringRegex  : The query parameters
        // (?:#.+)?"         : Optional fragment identifier
        // $                 : End of string
        var fullPattern =
            @"^" +
            @"https?://(?:(?<subdomain>[^.]+)\.)?" +
            domainRegex +
            routeRegex +
            queryStringRegex +
            "(?:#.+)?" + 
            "$";

        return new Regex(
            fullPattern,
            // IgnoreCase: URL schemes/domains are case-insensitive
            // Compiled: Optimizes performance for frequent execution (critical for message listeners)
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            // Timeout: Prevents "Catastrophic Backtracking" if a user sends a malicious string
            TimeSpan.FromMilliseconds(200)
        );
    }
}