using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Localization;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Deployment;

internal sealed class TranslationsDeploymentSelection
{
    private readonly ILocalizationService _localization;
    private readonly IEnumerable<ILocalizationDataProvider> _providers;

    public TranslationsDeploymentSelection(ILocalizationService localization, IEnumerable<ILocalizationDataProvider> providers)
    {
        _localization = localization;
        _providers = providers;
    }

    public Task<string[]> GetCulturesAsync() => _localization.GetSupportedCulturesAsync();

    public async Task<string[]> GetCategoriesAsync()
    {
        var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var provider in _providers)
        {
            foreach (var descriptor in await provider.GetDescriptorsAsync())
            {
                categories.Add(descriptor.Context);
            }
        }
        return [.. categories.OrderBy(category => category)];
    }

    public static void Apply(TranslationsDeploymentStep step, bool includeAll, string[] cultures, string[] categories)
    {
        step.IncludeAll = includeAll;
        step.Cultures = cultures ?? [];
        step.Categories = categories ?? [];
    }
}

internal sealed class TranslationsDeploymentStepDefinition : IDeploymentStepDefinition
{
    private readonly TranslationsDeploymentSelection _selection;

    public TranslationsDeploymentStepDefinition(ILocalizationService localization, IEnumerable<ILocalizationDataProvider> providers)
    {
        _selection = new TranslationsDeploymentSelection(localization, providers);
    }

    public string Type => nameof(TranslationsDeploymentStep);

    public JsonObject GetSchema() => new()
    {
        ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["includeAll"] = new JsonObject { ["type"] = "boolean" },
            ["cultures"] = NamesSchema(), ["categories"] = NamesSchema(),
        },
    };

    private static JsonObject NamesSchema() => new()
    {
        ["type"] = new JsonArray("array", "null"), ["maxItems"] = 1024,
        ["items"] = new JsonObject { ["type"] = "string", ["minLength"] = 1, ["maxLength"] = 256 },
    };

    public JsonObject Describe(DeploymentStep step)
    {
        var value = (TranslationsDeploymentStep)step;
        return new() { ["includeAll"] = value.IncludeAll, ["cultures"] = JArray.FromObject(value.Cultures ?? []), ["categories"] = JArray.FromObject(value.Categories ?? []) };
    }

    public async ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values)
    {
        var target = (TranslationsDeploymentStep)step;
        var includeAll = target.IncludeAll;
        var cultures = target.Cultures ?? [];
        var categories = target.Categories ?? [];
        var errors = new Dictionary<string, string[]>();
        if (values is null)
        {
            errors["values"] = ["Provide a settings object."];
            return errors;
        }
        foreach (var entry in values)
        {
            if (entry.Key == "includeAll" && entry.Value is JsonValue boolean && boolean.TryGetValue<bool>(out var flag))
            {
                includeAll = flag;
            }
            else if (entry.Key is "cultures" or "categories" && (entry.Value is null
                || entry.Value is JsonArray array && array.Count <= 1024 && array.All(item => item is JsonValue scalar
                    && scalar.TryGetValue<string>(out var name) && !string.IsNullOrWhiteSpace(name) && name.Length <= 256)))
            {
                var names = entry.Value?.AsArray().Select(item => item.GetValue<string>()).ToArray() ?? [];
                if (entry.Key == "cultures") { cultures = names; }
                else { categories = names; }
            }
            else
            {
                errors[entry.Key] = ["Provide a supported property matching the step schema."];
            }
        }
        if (errors.Count > 0) { return errors; }
        var supported = await _selection.GetCulturesAsync();
        if (!includeAll && cultures.Any(culture => !supported.Contains(culture, StringComparer.OrdinalIgnoreCase)))
        {
            errors["cultures"] = ["Select supported tenant cultures."];
        }
        var available = await _selection.GetCategoriesAsync();
        if (categories.Any(category => !available.Contains(category, StringComparer.OrdinalIgnoreCase)))
        {
            errors["categories"] = ["Select existing translation categories."];
        }
        if (errors.Count == 0)
        {
            TranslationsDeploymentSelection.Apply(target, includeAll, cultures, categories);
        }
        return errors;
    }
}
