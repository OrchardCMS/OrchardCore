using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Themes.Schemes.Models;
using OrchardCore.Themes.Schemes.Services;

namespace OrchardCore.Themes.Schemes.Deployment;

/// <summary>
/// Exports the schemes of every theme that supports them, the themes' defaults included, and the site's default
/// scheme of each theme.
/// </summary>
public sealed class ThemeSchemesDeploymentSource : DeploymentSourceBase<ThemeSchemesDeploymentStep>
{
    private readonly ThemeSchemeStore _store;

    public ThemeSchemesDeploymentSource(ThemeSchemeStore store)
    {
        _store = store;
    }

    protected override async Task ProcessAsync(ThemeSchemesDeploymentStep step, DeploymentPlanResult result)
    {
        var schemes = new JsonArray();
        var defaults = new JsonObject();

        foreach (var themeId in _store.ThemeIds.Order(StringComparer.Ordinal))
        {
            foreach (var scheme in await _store.ListAsync(themeId))
            {
                schemes.Add(ToEntry(themeId, scheme));
            }

            var defaultId = await _store.GetDefaultIdAsync(themeId);

            if (defaultId is not null)
            {
                defaults[themeId] = defaultId;
            }
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = ThemeSchemesDeploymentStep.StepName,
            [ThemeSchemesDeploymentStep.EntriesName] = schemes,
            [ThemeSchemesDeploymentStep.DefaultsName] = defaults,
        });
    }

    private static JsonObject ToEntry(string themeId, ThemeScheme scheme)
    {
        var entry = new JsonObject
        {
            [ThemeSchemesDeploymentStep.ThemeIdName] = themeId,
            [ThemeSchemesDeploymentStep.ItemIdName] = scheme.Id,
            [nameof(ThemeScheme.Name)] = scheme.Name,
            [nameof(ThemeScheme.Position)] = scheme.Position,
            [nameof(ThemeScheme.Primary)] = scheme.Primary,
            [nameof(ThemeScheme.Deep)] = scheme.Deep,
            [nameof(ThemeScheme.Ink)] = scheme.Ink,
            [nameof(ThemeScheme.Accent)] = scheme.Accent,
            [nameof(ThemeScheme.LightMenu)] = scheme.LightMenu,
        };

        // The optional settings only when they are set, so an export shows what was tuned.
        AddIfSet(entry, nameof(ThemeScheme.Hue), scheme.Hue);
        AddIfSet(entry, nameof(ThemeScheme.Saturation), scheme.Saturation);
        AddIfSet(entry, nameof(ThemeScheme.OnPrimary), scheme.OnPrimary);
        AddIfSet(entry, nameof(ThemeScheme.Page), scheme.Page);
        AddIfSet(entry, nameof(ThemeScheme.Toolbar), scheme.Toolbar);
        AddIfSet(entry, nameof(ThemeScheme.Tint), scheme.Tint);
        AddIfSet(entry, nameof(ThemeScheme.TintDark), scheme.TintDark);
        AddIfSet(entry, nameof(ThemeScheme.FontFamily), scheme.FontFamily);
        AddIfSet(entry, nameof(ThemeScheme.HeadingFontFamily), scheme.HeadingFontFamily);
        AddIfSet(entry, nameof(ThemeScheme.FontScale), scheme.FontScale);
        AddIfSet(entry, nameof(ThemeScheme.RadiusScale), scheme.RadiusScale);
        AddIfSet(entry, nameof(ThemeScheme.Density), scheme.Density);

        return entry;
    }

    private static void AddIfSet(JsonObject entry, string name, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            entry[name] = value;
        }
    }

    private static void AddIfSet(JsonObject entry, string name, double? value)
    {
        if (value is not null)
        {
            entry[name] = value.Value;
        }
    }
}
