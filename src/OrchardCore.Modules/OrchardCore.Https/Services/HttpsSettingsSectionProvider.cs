using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Https.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Https.Services;

internal sealed class HttpsSettingsSectionProvider : ISiteSettingsSectionProvider
{
    private readonly ISiteService _siteService;
    private readonly IHttpsService _httpsService;
    private readonly IShellReleaseManager _releaseManager;
    internal readonly IStringLocalizer S;

    public HttpsSettingsSectionProvider(ISiteService siteService, IHttpsService httpsService,
        IShellReleaseManager releaseManager, IStringLocalizer<HttpsSettingsSectionProvider> localizer)
    {
        _siteService = siteService;
        _httpsService = httpsService;
        _releaseManager = releaseManager;
        S = localizer;
    }

    public SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "https",
        DisplayName = "HTTPS",
        FeatureId = "OrchardCore.Https",
        Description = "Tenant HTTPS redirection and HSTS settings. Omitted properties retain their values; sslPort: null restores automatic port detection.",
        RequiresHttps = true,
        RequiresReload = true,
    };

    public Permission ReadPermission => Permissions.ManageHttps;
    public Permission UpdatePermission => Permissions.ManageHttps;

    public JsonObject GetSchema() => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["title"] = "HTTPS settings update",
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["description"] = "All fields are optional; omitted fields retain their values. Requests must use HTTPS.",
        ["properties"] = new JsonObject
        {
            ["strictTransportSecurityMode"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray("Disabled", "Enabled", "FromConfiguration"),
                ["description"] = "FromConfiguration enables HSTS in Production and disables it in other environments; it does not expose host HSTS options.",
            },
            ["requireHttps"] = new JsonObject { ["type"] = "boolean", ["description"] = "Redirect HTTP requests to HTTPS." },
            ["requireHttpsPermanent"] = new JsonObject { ["type"] = "boolean", ["description"] = "Use permanent 308 redirects instead of temporary 307 redirects when redirection is enabled." },
            ["sslPort"] = new JsonObject
            {
                ["type"] = new JsonArray("integer", "null"), ["minimum"] = 1, ["maximum"] = 65535,
                ["description"] = "Explicit HTTPS redirect port; null restores automatic detection.",
            },
        },
    };

    public async Task<SiteSettingsSectionResponse> GetAsync() => ToResponse(await _httpsService.GetSettingsAsync());

    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        if (values is null)
        {
            return new() { Errors = new() { ["body"] = [S["A settings object is required."]] } };
        }
        var site = await _siteService.LoadSiteSettingsAsync();
        var current = site.GetOrCreate<HttpsSettings>();
        var proposed = new HttpsSettings
        {
            StrictTransportSecurityMode = current.StrictTransportSecurityMode,
            RequireHttps = current.RequireHttps,
            RequireHttpsPermanent = current.RequireHttpsPermanent,
            SslPort = current.SslPort,
        };
        var errors = new Dictionary<string, string[]>();
        foreach (var entry in values)
        {
            switch (entry.Key)
            {
                case "strictTransportSecurityMode":
                    if (entry.Value is JsonValue modeValue && modeValue.TryGetValue<string>(out var name)
                        && Enum.GetNames<HttpStrictTransportSecurityMode>().Contains(name, StringComparer.OrdinalIgnoreCase)
                        && Enum.TryParse<HttpStrictTransportSecurityMode>(name, ignoreCase: true, out var mode))
                    {
                        proposed.StrictTransportSecurityMode = mode;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Select a valid HSTS mode by name: Disabled, Enabled or FromConfiguration."]];
                    }
                    break;
                case "requireHttps":
                case "requireHttpsPermanent":
                    if (entry.Value is JsonValue boolValue && boolValue.TryGetValue<bool>(out var enabled))
                    {
                        if (entry.Key == "requireHttps")
                        {
                            proposed.RequireHttps = enabled;
                        }
                        else
                        {
                            proposed.RequireHttpsPermanent = enabled;
                        }
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a Boolean value; null does not reset this property."]];
                    }
                    break;
                case "sslPort":
                    if (entry.Value is null)
                    {
                        proposed.SslPort = null;
                    }
                    else if (entry.Value is JsonValue portValue && portValue.TryGetValue<int>(out var port))
                    {
                        proposed.SslPort = port;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide an integer port or null for automatic detection."]];
                    }
                    break;
                default:
                    errors[entry.Key] = [S["This property is not managed by the HTTPS settings section."]];
                    break;
            }
        }
        foreach (var entry in HttpsSettingsEditor.Validate(proposed, S))
        {
            errors[char.ToLowerInvariant(entry.Key[0]) + entry.Key[1..]] = entry.Value;
        }
        if (errors.Count > 0)
        {
            return new() { Errors = errors };
        }
        var changed = HttpsSettingsEditor.Apply(current, proposed);
        if (changed)
        {
            site.Put(nameof(HttpsSettings), current);
            await _siteService.UpdateSiteSettingsAsync(site);
            _releaseManager.RequestRelease();
        }
        return new() { Section = ToResponse(current), Changed = changed, ReloadRequested = changed };
    }

    private static SiteSettingsSectionResponse ToResponse(HttpsSettings settings) => new()
    {
        Name = "https",
        Source = "tenant",
        Values = new JsonObject
        {
            ["strictTransportSecurityMode"] = settings.StrictTransportSecurityMode.ToString(),
            ["requireHttps"] = settings.RequireHttps,
            ["requireHttpsPermanent"] = settings.RequireHttpsPermanent,
            ["sslPort"] = settings.SslPort,
        },
    };
}
