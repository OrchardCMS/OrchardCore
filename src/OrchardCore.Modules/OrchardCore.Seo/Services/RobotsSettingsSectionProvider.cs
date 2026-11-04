using System.Text.Json.Nodes;
using OrchardCore.Entities;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Seo.Services;

internal sealed class RobotsSettingsSectionProvider : ISiteSettingsSectionProvider
{
    private readonly ISiteService _siteService;
    private readonly IStaticFileProvider _files;

    public RobotsSettingsSectionProvider(ISiteService siteService, IStaticFileProvider files)
    {
        _siteService = siteService;
        _files = files;
    }

    public SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "robots", DisplayName = "Robots.txt", FeatureId = "OrchardCore.Seo",
        Description = "Tenant robots.txt rules. Omission preserves values; null clears additionalRules. A physical robots.txt takes precedence.",
    };
    public Permission ReadPermission => SeoConstants.ManageSeoSettings;
    public Permission UpdatePermission => SeoConstants.ManageSeoSettings;
    public JsonObject GetSchema() => new()
    {
        ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["allowAllAgents"] = new JsonObject { ["type"] = "boolean" },
            ["disallowAdmin"] = new JsonObject { ["type"] = "boolean" },
            ["additionalRules"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
        },
    };
    public async Task<SiteSettingsSectionResponse> GetAsync() => Describe(await _siteService.GetSettingsAsync<RobotsSettings>());
    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        if (values is null) { return new() { Errors = new() { ["body"] = ["A settings object is required."] } }; }
        if (_files.GetFileInfo(SeoConstants.RobotsFileName).Exists)
        {
            return new() { Errors = new() { ["body"] = ["A physical robots.txt controls the public response. Remove it before updating tenant rules."] } };
        }
        var site = await _siteService.LoadSiteSettingsAsync();
        var current = site.GetOrCreate<RobotsSettings>();
        var allow = current.AllowAllAgents;
        var disallow = current.DisallowAdmin;
        var rules = current.AdditionalRules;
        var errors = new Dictionary<string, string[]>();
        foreach (var entry in values)
        {
            if (entry.Key is "allowAllAgents" or "disallowAdmin")
            {
                if (entry.Value is JsonValue value && value.TryGetValue<bool>(out var flag))
                {
                    if (entry.Key == "allowAllAgents") { allow = flag; } else { disallow = flag; }
                }
                else { errors[entry.Key] = ["A boolean value is required."]; }
            }
            else if (entry.Key == "additionalRules")
            {
                if (entry.Value is null) { rules = null; }
                else if (entry.Value is JsonValue value && value.TryGetValue<string>(out var text)) { rules = text; }
                else { errors[entry.Key] = ["Provide text or null to clear the additional rules."]; }
            }
            else { errors[entry.Key] = ["This robots setting is not writable."]; }
        }
        if (errors.Count > 0) { return new() { Errors = errors }; }
        var changed = RobotsSettingsEditor.Apply(current, allow, disallow, rules);
        if (changed)
        {
            site.Put(nameof(RobotsSettings), current);
            await _siteService.UpdateSiteSettingsAsync(site);
        }
        return new() { Section = Describe(current), Changed = changed };
    }
    private SiteSettingsSectionResponse Describe(RobotsSettings settings)
    {
        var physical = _files.GetFileInfo(SeoConstants.RobotsFileName).Exists;
        return new()
        {
            Name = "robots", Source = physical ? "configuration" : "tenant", IsReadOnly = physical,
            ReadOnlyReason = physical ? "A physical robots.txt takes precedence over tenant settings." : null,
            Values = new JsonObject { ["allowAllAgents"] = settings.AllowAllAgents, ["disallowAdmin"] = settings.DisallowAdmin,
                ["additionalRules"] = settings.AdditionalRules, },
        };
    }
}
