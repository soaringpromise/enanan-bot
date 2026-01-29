using System.Text.RegularExpressions;
// ReSharper disable NullableWarningSuppressionIsUsed

namespace EnananBot.Objects;

/// <summary>
/// A generic base implementation for website link fixers that use Regex-based routing.
/// Handles the logic of matching a URL against multiple possible route patterns 
/// and generating a fixed URL string.
/// </summary>
public abstract class GenericWebsiteLink : WebsiteLink
{
    // The successful Regex match result (if any)
    private readonly Match? _match;
    // The generated replacement string template (e.g., "https://{subdomain}fxtwitter.com/${id}")
    private readonly string? _replacementTemplate;

    protected GenericWebsiteLink(string url) : base(url)
    {
        // On initialization, try to find a matching route immediately
        (_match, _replacementTemplate) = GetMatchAndRepl();
    }
    
    // The display name of the fixer (e.g. "FxTwitter", "ddinstagram")
    protected abstract string FixerName { get; }
    
    // The target domain to replace with (e.g. "fxtwitter.com")
    protected abstract string FixDomain { get; }
    
    // A dictionary mapping abstract route definitions (keys) to compiled Regexes (values).
    protected abstract Dictionary<string, Regex> PossibleRoutes { get; }
    
    public override bool IsValid => _match is { Success: true };
    
    /// <summary>
    /// Iterates through all possible routes to find the best match for the current URL.
    /// </summary>
    private (Match?, string?) GetMatchAndRepl()
    {
        // Sort routes by length/complexity (descending) to ensure specific paths 
        // are matched before generic ones (e.g., match /user/status/123 before /user).
        foreach (var route in PossibleRoutes
                     .OrderByDescending(r => r.Key.Count(c => c == '/')))
        {
            var initialMatch = route.Value.Match(Url, 0, Url.Length);
            if (!initialMatch.Success)
                continue;

            // Allow child classes to clean up the URL before final matching
            var normalizedUrl = Normalize(initialMatch, Url);
            
            // Re-match against the normalized URL
            var finalMatch = route.Value.Match(normalizedUrl, 0, normalizedUrl.Length);
            if (!finalMatch.Success)
                continue;

            return (finalMatch, GetRepl(route.Key, finalMatch));
        }
        return (null, null);
    }

    // Hooks for child classes to handle subdomain logic (e.g., www vs. mobile)
    protected virtual string GetSubdomain(Match match) => "";
    
    // Hook for injecting specific subdomains into the fixed URL
    protected virtual string RouteFixSubdomain() => "";
    
    protected virtual string Normalize(Match match, string url) => url;

    /// <summary>
    /// Checks if the URL is already using the "fixed" domain to avoid loops.
    /// </summary>
    public virtual bool IsAlreadyFixed(Uri uri)
        => uri.Host.Equals(FixDomain, StringComparison.OrdinalIgnoreCase);
    
    /// <summary>
    /// Dynamically constructs a Regex Replacement Pattern based on the route definition.
    /// Converts a route like "/:id" into a replacement string like "${id}".
    /// </summary>
    protected virtual string GetRepl(string route, Match match)
    {
        if (!route.StartsWith('/'))
            route = "/" + route;

        // 1. Identify all named parameters defined in the route (e.g. "id", "username")
        var foundPathSegments =
            Regex.Matches(route, @":(\w+)(?:\([^/]+\))?")
                 .Select(m => m.Groups[1].Value)
                 .ToHashSet();

        // 2. Find any "extra" captured groups in the Regex match that weren't part of the path.
        // These are usually query parameters.
        var extraParams = match.Groups
            .Cast<Group>()
            .Where(g =>
                g.Success &&
                g.Name != "0" && // Skip the full match group
                !foundPathSegments.Contains(g.Name) &&
                g.Name is not ("domain" or "subdomain"))
            .Select(g => g.Name)
            .ToList();
        
        // 3. Remove optional parameter syntax from the route string
        route = Regex.Replace(route, @"/:(\w+)(?:\([^/]+\))?\?", "");

        // 4. Replace route parameters with Regex substitution tokens (e.g. :id -> ${id})
        var routeRepl = Regex.Replace(route, @":(\w+)(?:\([^/]+\))?", @"${$1}");
        
        // 5. Build the query string part if extra params exist
        var queryStringRepl = extraParams.Count > 0
            ? "?" + string.Join("&", extraParams.Select(p => $"{p}=${{{p}}}"))
            : "";

        // Combine into a final.NET Regex Replacement Pattern
        return "https://{subdomain}{domain}" +
               routeRepl +
               queryStringRepl;
    }
    
    /// <summary>
    /// Generates the final fixed URL by applying the template to the Regex match.
    /// </summary>
    public override Task<(string? FixedUrl, string? FixerName)> GetFixedUrlAsync()
    {
        if (!IsValid)
            return Task.FromResult<(string?, string?)>((null, null));

        // Inject the specific target domain (e.g., fxtwitter.com) and subdomain
        var patchedTemplate = _replacementTemplate!
            .Replace("{domain}", RouteFixSubdomain() + FixDomain)
            .Replace("{subdomain}", GetSubdomain(_match!));

        // Perform the regex substitution
        // _match.Result() replaces patterns like ${id} with the captured value "123"
        var fixedUrl = _match!.Result(patchedTemplate);

        return Task.FromResult<(string?, string?)>((fixedUrl, FixerName));
    }
}