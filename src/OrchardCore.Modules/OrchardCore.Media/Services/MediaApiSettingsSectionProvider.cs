using System.Text.Json.Nodes;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Media.Services;

internal sealed class MediaApiSettingsSectionProvider : ISiteSettingsSectionProvider
{
    private readonly ISiteService _siteService;
    private readonly IShellReleaseManager _release;

    public MediaApiSettingsSectionProvider(ISiteService siteService, IShellReleaseManager release)
    {
        _siteService = siteService;
        _release = release;
    }

    public SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "media-api", DisplayName = "Media API", FeatureId = "OrchardCore.Media", RequiresReload = true,
        Description = "The existing media gallery/file API cookie or bearer authentication selection. This does not provision OpenID.",
    };
    public Permission ReadPermission => MediaPermissions.ManageMediaApiSettings;
    public Permission UpdatePermission => MediaPermissions.ManageMediaApiSettings;

    public JsonObject GetSchema() => new()
    {
        ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["authenticationScheme"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("Cookie", "Bearer") },
        },
    };

    public async Task<SiteSettingsSectionResponse> GetAsync() => Describe(await _siteService.GetSettingsAsync<MediaApiSettings>());

    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        if (values is null) { return new() { Errors = new() { ["body"] = ["A settings object is required."] } }; }
        var site = await _siteService.LoadSiteSettingsAsync();
        var current = site.GetOrCreate<MediaApiSettings>();
        var proposed = current.AuthenticationScheme;
        var errors = new Dictionary<string, string[]>();
        foreach (var entry in values)
        {
            if (entry.Key != "authenticationScheme") { errors[entry.Key] = ["This property is not writable through the media API settings."]; }
            else if (entry.Value is JsonValue text && text.TryGetValue<string>(out var schemeName) && schemeName is "Cookie" or "Bearer"
                && Enum.TryParse<MediaApiAuthenticationScheme>(schemeName, out var scheme) && MediaApiSettingsEditor.IsValid(scheme))
            {
                proposed = scheme;
            }
            else { errors[entry.Key] = ["Select Cookie or Bearer by name."]; }
        }
        if (errors.Count > 0) { return new() { Errors = errors }; }
        var changed = MediaApiSettingsEditor.Apply(current, proposed);
        if (changed)
        {
            site.Put(current);
            await _siteService.UpdateSiteSettingsAsync(site);
            _release.RequestRelease();
        }
        return new() { Section = Describe(current), Changed = changed, ReloadRequested = changed };
    }

    private static SiteSettingsSectionResponse Describe(MediaApiSettings settings) => new()
    {
        Name = "media-api", Source = "tenant",
        Values = new JsonObject { ["authenticationScheme"] = settings.AuthenticationScheme.ToString() },
    };
}
