using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Security.Services;

internal sealed class SecuritySettingsSectionProvider : ISiteSettingsSectionProvider
{
    private readonly ISiteService _siteService;
    private readonly IOptionsSnapshot<SecuritySettings> _options;
    private readonly IShellReleaseManager _releaseManager;
    internal readonly IStringLocalizer S;

    public SecuritySettingsSectionProvider(ISiteService siteService, IOptionsSnapshot<SecuritySettings> options,
        IShellReleaseManager releaseManager, IStringLocalizer<SecuritySettingsSectionProvider> localizer)
    {
        _siteService = siteService;
        _options = options;
        _releaseManager = releaseManager;
        S = localizer;
    }

    public SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "security-headers",
        DisplayName = "Security headers",
        FeatureId = "OrchardCore.Security",
        Description = "Content security, permissions and referrer policies. Supplied policy maps replace the whole map; omitted properties retain their values.",
        RequiresReload = true,
    };

    public Permission ReadPermission => SecurityPermissions.ManageSecurityHeadersSettings;
    public Permission UpdatePermission => SecurityPermissions.ManageSecurityHeadersSettings;

    public JsonObject GetSchema() => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["title"] = "Security header settings update",
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["contentSecurityPolicy"] = new JsonObject
            {
                ["type"] = "object",
                ["additionalProperties"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
                ["description"] = "Complete replacement. Null omits a directive except sandbox and upgrade-insecure-requests, where it enables the flag. Upgrade-insecure-requests is always normalized to null.",
            },
            ["permissionsPolicy"] = new JsonObject
            {
                ["type"] = "object",
                ["additionalProperties"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Complete replacement. The editor's () sentinel omits a directive. Values use the existing permissions policy formatter.",
            },
            ["referrerPolicy"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray(SecuritySettingsEditor.ReferrerPolicies.Select(value => (JsonNode)JsonValue.Create(value)).ToArray()),
            },
        },
    };

    public Task<SiteSettingsSectionResponse> GetAsync() => Task.FromResult(ToResponse(_options.Value));

    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        if (_options.Value.FromConfiguration)
        {
            return new() { Errors = new() { ["body"] = [S["Security headers are owned by host configuration."]] } };
        }
        if (values is null)
        {
            return new() { Errors = new() { ["body"] = [S["A settings object is required."]] } };
        }
        var site = await _siteService.LoadSiteSettingsAsync();
        var current = site.GetOrCreate<SecuritySettings>();
        var proposed = new SecuritySettings
        {
            ContentTypeOptions = current.ContentTypeOptions,
            ContentSecurityPolicy = current.ContentSecurityPolicy,
            PermissionsPolicy = current.PermissionsPolicy,
            ReferrerPolicy = current.ReferrerPolicy,
        };
        var errors = new Dictionary<string, string[]>();
        foreach (var entry in values)
        {
            switch (entry.Key)
            {
                case "contentSecurityPolicy":
                case "permissionsPolicy":
                    if (entry.Value is not JsonObject policy)
                    {
                        errors[entry.Key] = [S["Provide a directive object; an empty object removes the policy."]];
                        break;
                    }
                    var directives = new Dictionary<string, string>();
                    foreach (var directive in policy)
                    {
                        if (directive.Value is JsonValue text && text.TryGetValue<string>(out var value))
                        {
                            directives[directive.Key] = value;
                        }
                        else if (directive.Value is null && entry.Key == "contentSecurityPolicy")
                        {
                            directives[directive.Key] = null;
                        }
                        else
                        {
                            errors[entry.Key + "." + directive.Key] = [S["Provide a string directive value."]];
                        }
                    }
                    if (entry.Key == "contentSecurityPolicy")
                    {
                        proposed.ContentSecurityPolicy = directives;
                    }
                    else
                    {
                        proposed.PermissionsPolicy = directives;
                    }
                    break;
                case "referrerPolicy":
                    if (entry.Value is JsonValue referrer && referrer.TryGetValue<string>(out var referrerPolicy))
                    {
                        proposed.ReferrerPolicy = referrerPolicy;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a referrer policy name."]];
                    }
                    break;
                default:
                    errors[entry.Key] = [S["This property is not managed by the security headers section."]];
                    break;
            }
        }
        foreach (var entry in SecuritySettingsEditor.Validate(proposed, S))
        {
            errors[char.ToLowerInvariant(entry.Key[0]) + entry.Key[1..]] = entry.Value;
        }
        if (errors.Count > 0)
        {
            return new() { Errors = errors };
        }
        var changed = SecuritySettingsEditor.Apply(current, proposed);
        if (changed)
        {
            site.Put(nameof(SecuritySettings), current);
            await _siteService.UpdateSiteSettingsAsync(site);
            _releaseManager.RequestRelease();
        }
        return new() { Section = ToResponse(current), Changed = changed, ReloadRequested = changed };
    }

    private static SiteSettingsSectionResponse ToResponse(SecuritySettings settings) => new()
    {
        Name = "security-headers",
        Source = settings.FromConfiguration ? "configuration" : "tenant",
        IsReadOnly = settings.FromConfiguration,
        Values = new JsonObject
        {
            ["contentSecurityPolicy"] = ToObject(settings.ContentSecurityPolicy),
            ["permissionsPolicy"] = ToObject(settings.PermissionsPolicy),
            ["referrerPolicy"] = settings.ReferrerPolicy,
        },
    };

    private static JsonObject ToObject(Dictionary<string, string> values)
        => new(values.Select(entry => new KeyValuePair<string, JsonNode>(entry.Key, JsonValue.Create(entry.Value))));
}
