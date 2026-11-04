using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Cors.Settings;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Cors.Services;

/// <summary>Reads and validates tenant CORS policies for the admin editor, settings API and runtime.</summary>
public class CorsService
{
    private readonly ISiteService _siteService;
    internal readonly IStringLocalizer S;

    /// <summary>Creates a tenant CORS settings service.</summary>
    public CorsService(ISiteService siteService, IStringLocalizer<CorsService> localizer)
    {
        _siteService = siteService;
        S = localizer;
    }

    /// <summary>Reads the tenant-owned CORS settings.</summary>
    public Task<CorsSettings> GetSettingsAsync() => _siteService.GetSettingsAsync<CorsSettings>();

    internal Dictionary<string, string[]> Validate(CorsSettings settings)
    {
        var errors = new Dictionary<string, string[]>();
        if (settings?.Policies is null)
        {
            errors["policies"] = [S["Provide a policies array; use an empty array to remove all tenant policies."]];
            return errors;
        }
        var policies = settings.Policies.ToArray();
        var names = new HashSet<string>(StringComparer.Ordinal);
        var defaults = 0;
        for (var index = 0; index < policies.Length; index++)
        {
            var policy = policies[index];
            foreach (var error in ValidatePolicy(policy))
            {
                errors[$"policies[{index}].{error.Key}"] = error.Value;
            }
            if (policy is null)
            {
                continue;
            }
            if (!names.Add(policy.Name))
            {
                errors[$"policies[{index}].name"] = [S["Policy names must be unique."]];
            }
            if (policy.IsDefaultPolicy && ++defaults > 1)
            {
                errors[$"policies[{index}].isDefaultPolicy"] = [S["Select at most one default policy."]];
            }
        }
        return errors;
    }

    internal Dictionary<string, string[]> ValidatePolicy(CorsPolicySetting policy)
    {
        var errors = new Dictionary<string, string[]>();
        if (policy is null)
        {
            errors["policy"] = [S["A policy object is required."]];
            return errors;
        }
        if (string.IsNullOrWhiteSpace(policy.Name) || policy.Name.Length > 256 || policy.Name != policy.Name.Trim() || policy.Name.Any(char.IsControl))
        {
            errors["name"] = [S["Provide a policy name of at most 256 characters without surrounding whitespace or control characters."]];
        }
        if (policy.AllowCredentials && (policy.AllowAnyOrigin || policy.AllowedOrigins?.Contains("*", StringComparer.Ordinal) == true))
        {
            errors["allowCredentials"] = [S["Credentials cannot be combined with any origin, including a literal '*' allowed origin."]];
        }
        if (policy.AllowedOrigins?.Any(origin => !IsOrigin(origin)) == true)
        {
            errors["allowedOrigins"] = [S["Origins must be absolute HTTP or HTTPS origins without user information, a path, a trailing slash, query or fragment; '*' is allowed only without credentials."]];
        }
        if (policy.AllowedMethods?.Any(method => !IsToken(method)) == true)
        {
            errors["allowedMethods"] = [S["Methods must be nonempty HTTP tokens."]];
        }
        if (policy.AllowedHeaders?.Any(header => !IsToken(header)) == true)
        {
            errors["allowedHeaders"] = [S["Allowed headers must be nonempty HTTP header-name tokens."]];
        }
        if (policy.ExposedHeaders?.Any(header => !IsToken(header)) == true)
        {
            errors["exposedHeaders"] = [S["Exposed headers must be nonempty HTTP header-name tokens."]];
        }
        return errors;
    }

    internal async Task<CorsSettingsUpdateResult> UpdateSettingsAsync(CorsSettings corsSettings)
    {
        var errors = Validate(corsSettings);
        if (errors.Count > 0)
        {
            return new() { Errors = errors };
        }
        var proposed = Normalize(corsSettings);
        var siteSettings = await _siteService.LoadSiteSettingsAsync();
        var current = Normalize(siteSettings.GetOrCreate<CorsSettings>());
        if (JsonNode.DeepEquals(JsonSerializer.SerializeToNode(current), JsonSerializer.SerializeToNode(proposed)))
        {
            return new() { Settings = proposed };
        }
        siteSettings.Put(nameof(CorsSettings), proposed);
        await _siteService.UpdateSiteSettingsAsync(siteSettings);
        return new() { Settings = proposed, Changed = true };
    }

    private static CorsSettings Normalize(CorsSettings settings) => new()
    {
        Policies = (settings.Policies ?? []).Select(policy => policy is null ? null : new CorsPolicySetting
        {
            Name = policy.Name, IsDefaultPolicy = policy.IsDefaultPolicy,
            AllowAnyOrigin = policy.AllowAnyOrigin, AllowCredentials = policy.AllowCredentials,
            AllowAnyMethod = policy.AllowAnyMethod, AllowAnyHeader = policy.AllowAnyHeader,
            AllowedOrigins = policy.AllowedOrigins?.ToArray() ?? [], AllowedMethods = policy.AllowedMethods?.ToArray() ?? [],
            AllowedHeaders = policy.AllowedHeaders?.ToArray() ?? [], ExposedHeaders = policy.ExposedHeaders?.ToArray() ?? [],
        }).ToArray(),
    };

    private static bool IsOrigin(string origin)
    {
        if (origin == "*")
        {
            return true;
        }
        if (string.IsNullOrEmpty(origin) || origin.Any(char.IsWhiteSpace) || origin.Any(char.IsControl)
            || !Uri.TryCreate(origin, UriKind.Absolute, out var uri) || !uri.IsWellFormedOriginalString()
            || uri.Scheme is not ("http" or "https") || string.IsNullOrEmpty(uri.Host)
            || uri.Host.Contains('*') || !string.IsNullOrEmpty(uri.UserInfo))
        {
            return false;
        }
        var prefix = uri.Scheme + "://";
        // Check the original authority boundary, not a canonicalized path that can erase dot segments.
        return origin.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            && origin[prefix.Length..].IndexOfAny(['/', '\\', '?', '#']) < 0;
    }

    private static bool IsToken(string value) => !string.IsNullOrEmpty(value)
        && value.All(character => char.IsAsciiLetterOrDigit(character) || "!#$%&'*+-.^_`|~".Contains(character));
}

internal sealed class CorsSettingsUpdateResult
{
    public CorsSettings Settings { get; init; }
    public bool Changed { get; init; }
    public Dictionary<string, string[]> Errors { get; init; } = [];
}
