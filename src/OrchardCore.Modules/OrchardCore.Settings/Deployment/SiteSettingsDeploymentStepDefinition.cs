using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment;

internal static class SiteSettingsDeploymentSelection
{
    private static readonly Dictionary<string, Func<ISite, JsonNode>> s_values = new(StringComparer.Ordinal)
    {
        ["BaseUrl"] = site => JsonValue.Create(site.BaseUrl),
        ["Calendar"] = site => JsonValue.Create(site.Calendar),
        ["MaxPagedCount"] = site => JsonValue.Create(site.MaxPagedCount),
        ["MaxPageSize"] = site => JsonValue.Create(site.MaxPageSize),
        ["PageSize"] = site => JsonValue.Create(site.PageSize),
        ["AllowPageSizeSelection"] = site => JsonValue.Create(site.AllowPageSizeSelection),
        ["PageSizeOptions"] = site => site.PageSizeOptions is null ? null : JArray.FromObject(site.PageSizeOptions),
        ["ResourceDebugMode"] = site => JsonValue.Create(site.ResourceDebugMode),
        ["SiteName"] = site => JsonValue.Create(site.SiteName),
        ["PageTitleFormat"] = site => JsonValue.Create(site.PageTitleFormat),
        ["SiteSalt"] = site => JsonValue.Create(site.SiteSalt),
        ["SuperUser"] = site => JsonValue.Create(site.SuperUser),
        ["TimeZoneId"] = site => JsonValue.Create(site.TimeZoneId),
        ["UseCdn"] = site => JsonValue.Create(site.UseCdn),
        ["CdnBaseUrl"] = site => JsonValue.Create(site.CdnBaseUrl),
        ["AppendVersion"] = site => JsonValue.Create(site.AppendVersion),
        ["HomeRoute"] = site => JObject.FromObject(site.HomeRoute),
        ["CacheMode"] = site => JsonValue.Create(site.CacheMode),
    };

    public static IEnumerable<string> Names => s_values.Keys;

    public static string[] Normalize(string[] names) => (names ?? []).Distinct(StringComparer.Ordinal).ToArray();

    public static JsonObject Export(ISite site, string[] names)
    {
        var result = new JsonObject { ["name"] = "Settings" };
        foreach (var name in Normalize(names))
        {
            if (!s_values.TryGetValue(name, out var value))
            {
                throw new InvalidOperationException($"Unsupported setting '{name}'");
            }
            result[name] = value(site);
        }
        return result;
    }
}

internal sealed class SiteSettingsDeploymentStepDefinition : IDeploymentStepDefinition
{
    public string Type => nameof(SiteSettingsDeploymentStep);

    public JsonObject GetSchema() => new()
    {
        ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["settings"] = new JsonObject
            {
                ["type"] = new JsonArray("array", "null"), ["maxItems"] = 1024,
                ["items"] = new JsonObject { ["type"] = "string", ["enum"] = JArray.FromObject(SiteSettingsDeploymentSelection.Names) },
            },
        },
    };

    public JsonObject Describe(DeploymentStep step) => new()
    {
        ["settings"] = JArray.FromObject(((SiteSettingsDeploymentStep)step).Settings ?? []),
    };

    public ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values)
    {
        var errors = new Dictionary<string, string[]>();
        var target = (SiteSettingsDeploymentStep)step;
        var names = target.Settings;
        if (values is null || values.Any(entry => entry.Key != "settings"))
        {
            errors["values"] = ["Provide only the settings selection."];
        }
        else if (values.TryGetPropertyValue("settings", out var node))
        {
            if (node is null) { names = []; }
            else if (node is JsonArray array && array.Count <= 1024 && array.All(item => item is JsonValue scalar
                && scalar.TryGetValue<string>(out var name) && SiteSettingsDeploymentSelection.Names.Contains(name, StringComparer.Ordinal)))
            {
                names = array.Select(item => item.GetValue<string>()).ToArray();
            }
            else { errors["settings"] = ["Select supported site settings names."]; }
        }
        if (errors.Count == 0) { target.Settings = SiteSettingsDeploymentSelection.Normalize(names); }
        return ValueTask.FromResult<IReadOnlyDictionary<string, string[]>>(errors);
    }
}
