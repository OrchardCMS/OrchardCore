using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Services;

namespace OrchardCore.Themes.TheAdmin.Deployment;

/// <summary>
/// Exports every scheme of TheAdmin, the theme's own ones included, and the site's default scheme.
/// </summary>
public sealed class AdminSchemesDeploymentSource : DeploymentSourceBase<AdminSchemesDeploymentStep>
{
    private readonly AdminSchemeStore _store;

    public AdminSchemesDeploymentSource(AdminSchemeStore store)
    {
        _store = store;
    }

    protected override async Task ProcessAsync(AdminSchemesDeploymentStep step, DeploymentPlanResult result)
    {
        var schemes = new JsonArray();

        foreach (var scheme in await _store.ListAsync())
        {
            schemes.Add(ToEntry(scheme));
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = AdminSchemesDeploymentStep.StepName,
            [AdminSchemesDeploymentStep.EntriesName] = schemes,
            [AdminSchemesDeploymentStep.DefaultSchemeIdName] = await _store.GetDefaultIdAsync(),
        });
    }

    private static JsonObject ToEntry(AdminScheme scheme)
    {
        var entry = new JsonObject
        {
            [AdminSchemesDeploymentStep.ItemIdName] = scheme.Id,
            [nameof(AdminScheme.Name)] = scheme.Name,
            [nameof(AdminScheme.Position)] = scheme.Position,
            [nameof(AdminScheme.Primary)] = scheme.Primary,
            [nameof(AdminScheme.Deep)] = scheme.Deep,
            [nameof(AdminScheme.Ink)] = scheme.Ink,
            [nameof(AdminScheme.Accent)] = scheme.Accent,
            [nameof(AdminScheme.LightMenu)] = scheme.LightMenu,
        };

        // The optional settings only when they are set, so an export shows what was tuned.
        AddIfSet(entry, nameof(AdminScheme.Hue), scheme.Hue);
        AddIfSet(entry, nameof(AdminScheme.Saturation), scheme.Saturation);
        AddIfSet(entry, nameof(AdminScheme.OnPrimary), scheme.OnPrimary);
        AddIfSet(entry, nameof(AdminScheme.Page), scheme.Page);
        AddIfSet(entry, nameof(AdminScheme.Toolbar), scheme.Toolbar);
        AddIfSet(entry, nameof(AdminScheme.Tint), scheme.Tint);
        AddIfSet(entry, nameof(AdminScheme.TintDark), scheme.TintDark);
        AddIfSet(entry, nameof(AdminScheme.FontFamily), scheme.FontFamily);
        AddIfSet(entry, nameof(AdminScheme.HeadingFontFamily), scheme.HeadingFontFamily);
        AddIfSet(entry, nameof(AdminScheme.FontScale), scheme.FontScale);
        AddIfSet(entry, nameof(AdminScheme.RadiusScale), scheme.RadiusScale);
        AddIfSet(entry, nameof(AdminScheme.Density), scheme.Density);

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
