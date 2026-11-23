using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Themes.Schemes.Deployment;
using OrchardCore.Themes.Schemes.Models;
using OrchardCore.Themes.Schemes.Services;

namespace OrchardCore.Themes.Schemes.Recipes;

/// <summary>
/// The 'ThemeSchemes' recipe step. Each entry names the theme of its scheme ('ThemeId'): a scheme whose 'ItemId'
/// matches a stored one of that theme replaces it, any other is added (with a new id when it has none). The optional
/// 'Defaults' object maps a theme id to the id of the site's default scheme of the theme. Entries and defaults of a
/// theme that does not support schemes are skipped, with an error.
/// </summary>
public sealed class ThemeSchemesStep : NamedRecipeStepHandler
{
    private readonly ThemeSchemeStore _store;

    internal readonly IStringLocalizer S;

    public ThemeSchemesStep(ThemeSchemeStore store, IStringLocalizer<ThemeSchemesStep> stringLocalizer)
        : base(ThemeSchemesDeploymentStep.StepName)
    {
        _store = store;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        if (context.Step[ThemeSchemesDeploymentStep.EntriesName] is JsonArray entries)
        {
            var positions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in entries.OfType<JsonObject>())
            {
                await ImportAsync(context, entry, positions);
            }
        }

        if (context.Step[ThemeSchemesDeploymentStep.DefaultsName] is JsonObject defaults)
        {
            foreach (var (themeId, value) in defaults)
            {
                var id = value?.GetValue<string>();

                if (!_store.IsRegistered(themeId))
                {
                    context.Errors.Add(S["The theme '{0}' does not support schemes, or is not enabled: its default scheme was skipped.", themeId]);
                    continue;
                }

                if (!await _store.SetDefaultAsync(themeId, id))
                {
                    context.Errors.Add(S["The theme '{0}' has no scheme '{1}' to make its default scheme.", themeId, id]);
                }
            }
        }
    }

    private async Task ImportAsync(RecipeExecutionContext context, JsonObject entry, Dictionary<string, int> positions)
    {
        var themeId = entry[ThemeSchemesDeploymentStep.ThemeIdName]?.GetValue<string>();
        var id = entry[ThemeSchemesDeploymentStep.ItemIdName]?.GetValue<string>();

        if (!_store.IsRegistered(themeId))
        {
            context.Errors.Add(S["The theme '{0}' does not support schemes, or is not enabled: the scheme '{1}' was skipped.", themeId, id]);

            return;
        }

        if (string.IsNullOrEmpty(id))
        {
            id = ThemeSchemeStore.NewId();
        }

        if (!positions.TryGetValue(themeId, out var position))
        {
            position = await _store.NextPositionAsync(themeId);
        }

        var scheme = new ThemeScheme
        {
            Id = id,
            Name = entry[nameof(ThemeScheme.Name)]?.GetValue<string>()?.Trim(),
            Primary = entry[nameof(ThemeScheme.Primary)]?.GetValue<string>(),
            Deep = entry[nameof(ThemeScheme.Deep)]?.GetValue<string>(),
            Ink = entry[nameof(ThemeScheme.Ink)]?.GetValue<string>(),
            Accent = entry[nameof(ThemeScheme.Accent)]?.GetValue<string>(),
            LightMenu = entry[nameof(ThemeScheme.LightMenu)]?.GetValue<bool>() ?? false,
            Position = entry[nameof(ThemeScheme.Position)]?.GetValue<int>() ?? position++,
            Hue = entry[nameof(ThemeScheme.Hue)]?.GetValue<double>(),
            Saturation = entry[nameof(ThemeScheme.Saturation)]?.GetValue<double>(),
            OnPrimary = entry[nameof(ThemeScheme.OnPrimary)]?.GetValue<string>(),
            Page = entry[nameof(ThemeScheme.Page)]?.GetValue<string>(),
            Toolbar = entry[nameof(ThemeScheme.Toolbar)]?.GetValue<string>(),
            Tint = entry[nameof(ThemeScheme.Tint)]?.GetValue<string>(),
            TintDark = entry[nameof(ThemeScheme.TintDark)]?.GetValue<string>(),
            FontFamily = entry[nameof(ThemeScheme.FontFamily)]?.GetValue<string>(),
            HeadingFontFamily = entry[nameof(ThemeScheme.HeadingFontFamily)]?.GetValue<string>(),
            FontScale = entry[nameof(ThemeScheme.FontScale)]?.GetValue<double>(),
            RadiusScale = entry[nameof(ThemeScheme.RadiusScale)]?.GetValue<double>(),
            Density = entry[nameof(ThemeScheme.Density)]?.GetValue<double>(),
        };

        positions[themeId] = position;

        if (!ThemeSchemeStore.IsValidId(scheme.Id))
        {
            context.Errors.Add(S["The scheme id '{0}' is not valid: it must start with a lowercase letter and hold only lowercase letters, digits and dashes.", scheme.Id]);

            return;
        }

        if (string.IsNullOrEmpty(scheme.Name) || !ThemeSchemeStore.HasValidColors(scheme))
        {
            context.Errors.Add(S["The scheme '{0}' needs a name, four '#rrggbb' colors (Primary, Deep, Ink, Accent), and '#rrggbb' optional colors.", scheme.Id]);

            return;
        }

        if (!ThemeSchemeStore.HasValidTypography(scheme))
        {
            context.Errors.Add(S["The scheme '{0}' has a FontFamily or HeadingFontFamily that is not a font name or a list of them separated by commas (letters, digits, spaces, dashes, underscores and dots), or a FontScale (0.875 to 1.125), RadiusScale (0 to 1.5) or Density (0.75 to 1.25) out of range.", scheme.Id]);

            return;
        }

        await _store.SaveAsync(themeId, scheme);
    }
}
