using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Layers.Models;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Layers.Services;

internal sealed class LayerSettingsSectionProvider : ISiteSettingsSectionProvider
{
    private readonly ISiteService _siteService;
    internal readonly IStringLocalizer S;

    public LayerSettingsSectionProvider(ISiteService siteService, IStringLocalizer<LayerSettingsSectionProvider> localizer)
    {
        _siteService = siteService;
        S = localizer;
    }

    public SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "layer-zones",
        DisplayName = "Layer zones",
        FeatureId = "OrchardCore.Layers",
        Description = "Available widget zones. Omission preserves the list; an empty array clears it. Names use the admin editor's space/comma separators. Existing widgets are not moved or deleted.",
    };

    public Permission ReadPermission => Permissions.ManageLayers;
    public Permission UpdatePermission => Permissions.ManageLayers;

    public JsonObject GetSchema() => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["title"] = "Layer zones settings update",
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["zones"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Replaces the complete list. Each string is split on spaces and commas, discarding empty names. Order, case and duplicates are preserved. Null is invalid; [] clears the list.",
            },
        },
    };

    public async Task<SiteSettingsSectionResponse> GetAsync() => ToResponse(await _siteService.GetSettingsAsync<LayerSettings>());

    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        var errors = new Dictionary<string, string[]>();
        string[] zones = null;
        if (values is null)
        {
            errors["body"] = [S["A settings object is required."]];
        }
        else
        {
            foreach (var entry in values)
            {
                if (entry.Key != "zones")
                {
                    errors[entry.Key] = [S["This property is not managed by the layer zones settings section."]];
                }
                else if (entry.Value is JsonArray array && array.All(value => value is JsonValue item && item.TryGetValue<string>(out _)))
                {
                    zones = array.Select(value => value.GetValue<string>()).ToArray();
                }
                else
                {
                    errors["zones"] = [S["Provide an array of strings; use an empty array to clear the zones."]];
                }
            }
        }
        if (errors.Count > 0)
        {
            return new() { Errors = errors };
        }

        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<LayerSettings>();
        var changed = zones is not null && LayerSettingsEditor.Apply(settings, zones);
        if (changed)
        {
            site.Put(nameof(LayerSettings), settings);
            await _siteService.UpdateSiteSettingsAsync(site);
        }

        return new() { Section = ToResponse(settings), Changed = changed };
    }

    private static SiteSettingsSectionResponse ToResponse(LayerSettings settings) => new()
    {
        Name = "layer-zones",
        Source = "tenant",
        Values = new JsonObject
        {
            ["zones"] = new JsonArray((settings.Zones ?? []).Select(zone => (JsonNode)JsonValue.Create(zone)).ToArray()),
        },
    };
}
