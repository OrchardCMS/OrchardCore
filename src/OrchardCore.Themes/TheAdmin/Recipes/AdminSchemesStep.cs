using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Themes.TheAdmin.Deployment;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Services;

namespace OrchardCore.Themes.TheAdmin.Recipes;

/// <summary>
/// The 'AdminSchemes' recipe step. A scheme whose 'ItemId' matches a stored one replaces it, any other is added (with a
/// new id when it has none). The optional 'DefaultSchemeId' makes a scheme the site's default scheme.
/// </summary>
public sealed class AdminSchemesStep : NamedRecipeStepHandler
{
    private readonly AdminSchemeStore _store;

    internal readonly IStringLocalizer S;

    public AdminSchemesStep(AdminSchemeStore store, IStringLocalizer<AdminSchemesStep> stringLocalizer)
        : base(AdminSchemesDeploymentStep.StepName)
    {
        _store = store;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        if (context.Step[AdminSchemesDeploymentStep.EntriesName] is JsonArray entries)
        {
            var position = await _store.NextPositionAsync();

            foreach (var entry in entries.OfType<JsonObject>())
            {
                position = await ImportAsync(context, entry, position);
            }
        }

        var defaultId = context.Step[AdminSchemesDeploymentStep.DefaultSchemeIdName]?.GetValue<string>();

        if (!string.IsNullOrEmpty(defaultId) && !await _store.SetDefaultAsync(defaultId))
        {
            context.Errors.Add(S["There is no scheme '{0}' to make the default scheme.", defaultId]);
        }
    }

    // Returns the position of the next entry that names none.
    private async Task<int> ImportAsync(RecipeExecutionContext context, JsonObject entry, int position)
    {
        var id = entry[AdminSchemesDeploymentStep.ItemIdName]?.GetValue<string>();

        if (string.IsNullOrEmpty(id))
        {
            id = AdminSchemeStore.NewId();
        }

        var scheme = new AdminScheme
        {
            Id = id,
            Name = entry[nameof(AdminScheme.Name)]?.GetValue<string>()?.Trim(),
            Primary = entry[nameof(AdminScheme.Primary)]?.GetValue<string>(),
            Deep = entry[nameof(AdminScheme.Deep)]?.GetValue<string>(),
            Ink = entry[nameof(AdminScheme.Ink)]?.GetValue<string>(),
            Accent = entry[nameof(AdminScheme.Accent)]?.GetValue<string>(),
            LightMenu = entry[nameof(AdminScheme.LightMenu)]?.GetValue<bool>() ?? false,
            Position = entry[nameof(AdminScheme.Position)]?.GetValue<int>() ?? position++,
            Hue = entry[nameof(AdminScheme.Hue)]?.GetValue<double>(),
            Saturation = entry[nameof(AdminScheme.Saturation)]?.GetValue<double>(),
            OnPrimary = entry[nameof(AdminScheme.OnPrimary)]?.GetValue<string>(),
            Page = entry[nameof(AdminScheme.Page)]?.GetValue<string>(),
            Toolbar = entry[nameof(AdminScheme.Toolbar)]?.GetValue<string>(),
            Tint = entry[nameof(AdminScheme.Tint)]?.GetValue<string>(),
            TintDark = entry[nameof(AdminScheme.TintDark)]?.GetValue<string>(),
            FontFamily = entry[nameof(AdminScheme.FontFamily)]?.GetValue<string>(),
            HeadingFontFamily = entry[nameof(AdminScheme.HeadingFontFamily)]?.GetValue<string>(),
            FontScale = entry[nameof(AdminScheme.FontScale)]?.GetValue<double>(),
            RadiusScale = entry[nameof(AdminScheme.RadiusScale)]?.GetValue<double>(),
            Density = entry[nameof(AdminScheme.Density)]?.GetValue<double>(),
        };

        if (!AdminSchemeStore.IsValidId(scheme.Id))
        {
            context.Errors.Add(S["The scheme id '{0}' is not valid: it must start with a lowercase letter and hold only lowercase letters, digits and dashes.", scheme.Id]);

            return position;
        }

        if (string.IsNullOrEmpty(scheme.Name) || !AdminSchemeStore.HasValidColors(scheme))
        {
            context.Errors.Add(S["The scheme '{0}' needs a name, four '#rrggbb' colors (Primary, Deep, Ink, Accent), and '#rrggbb' optional colors.", scheme.Id]);

            return position;
        }

        if (!AdminSchemeStore.HasValidTypography(scheme))
        {
            context.Errors.Add(S["The scheme '{0}' has a FontFamily or HeadingFontFamily that is not a font name or a list of them separated by commas (letters, digits, spaces, dashes, underscores and dots), or a FontScale (0.875 to 1.125), RadiusScale (0 to 1.5) or Density (0.75 to 1.25) out of range.", scheme.Id]);

            return position;
        }

        await _store.SaveAsync(scheme);

        return position;
    }
}
