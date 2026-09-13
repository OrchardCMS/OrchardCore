using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Search.Models;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Search.Services;

internal sealed class SearchSettingsSectionProvider : ISiteSettingsSectionProvider
{
    private readonly ISiteService _site;
    private readonly IIndexProfileStore _profiles;
    internal readonly IStringLocalizer S;

    public SearchSettingsSectionProvider(ISiteService site, IIndexProfileStore profiles, IStringLocalizer<SearchSettingsSectionProvider> localizer)
    {
        _site = site;
        _profiles = profiles;
        S = localizer;
    }

    public SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "frontend-search", DisplayName = "Frontend search", FeatureId = "OrchardCore.Search",
        Description = "Default administrative index name, page title and placeholder. Omitted properties retain their values; null clears an override. Does not grant query permissions or rebuild indexes.",
    };

    public Permission ReadPermission => SearchPermissions.ManageSearchSettings;
    public Permission UpdatePermission => SearchPermissions.ManageSearchSettings;

    public JsonObject GetSchema() => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["defaultIndexProfileName"] = Property("Administrative profile name, not its ID or provider resource name. Null/blank clears the default; new selections must exist in this tenant."),
            ["placeholder"] = Property("Search form placeholder; null clears the override."),
            ["pageTitle"] = Property("Search page title; null clears the override."),
        },
    };

    private static JsonObject Property(string description) => new()
    {
        ["type"] = new JsonArray("string", "null"), ["description"] = description,
    };

    public async Task<SiteSettingsSectionResponse> GetAsync() => Describe(await _site.GetSettingsAsync<SearchSettings>());

    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        var errors = new Dictionary<string, string[]>();
        if (values is null) { errors["body"] = [S["A settings object is required."]]; }
        else
        {
            foreach (var (name, value) in values)
            {
                if (name is not ("defaultIndexProfileName" or "placeholder" or "pageTitle"))
                {
                    errors[name] = [S["This property is not managed by frontend search settings."]];
                }
                else if (value is not null && (value is not JsonValue scalar || !scalar.TryGetValue<string>(out _)))
                {
                    errors[name] = [S["Provide a string or null to clear this value."]];
                }
            }
        }
        if (errors.Count > 0) { return new() { Errors = errors }; }

        var site = await _site.LoadSiteSettingsAsync();
        var current = site.GetOrCreate<SearchSettings>();
        var proposed = new SearchSettings
        {
            DefaultIndexProfileName = Read(values, "defaultIndexProfileName", current.DefaultIndexProfileName),
            Placeholder = Read(values, "placeholder", current.Placeholder),
            PageTitle = Read(values, "pageTitle", current.PageTitle),
        };
        if (!await SearchSettingsEditor.ValidateAsync(_profiles, current, proposed))
        {
            return new() { Errors = new() { ["defaultIndexProfileName"] = [S["Choose an existing index profile."]] } };
        }
        var changed = SearchSettingsEditor.Apply(current, proposed);
        if (changed)
        {
            site.Put(nameof(SearchSettings), current);
            await _site.UpdateSiteSettingsAsync(site);
        }
        return new() { Changed = changed, Section = Describe(current) };
    }

    private static string Read(JsonObject values, string name, string current) =>
        values.TryGetPropertyValue(name, out var value) ? value?.GetValue<string>() : current;

    private static SiteSettingsSectionResponse Describe(SearchSettings settings) => new()
    {
        Name = "frontend-search", Source = "tenant",
        Values = new JsonObject
        {
            ["defaultIndexProfileName"] = settings.DefaultIndexProfileName,
            ["placeholder"] = settings.Placeholder,
            ["pageTitle"] = settings.PageTitle,
        },
    };
}
