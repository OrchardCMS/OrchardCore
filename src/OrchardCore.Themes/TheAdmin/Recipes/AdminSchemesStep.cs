using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Themes.TheAdmin.Deployment;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Services;

namespace OrchardCore.Themes.TheAdmin.Recipes;

/// <summary>
/// The 'AdminSchemes' recipe step: a scheme whose ItemId matches a stored one replaces it, any other is added (with a
/// new id when it has none).
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
        if (context.Step[AdminSchemesDeploymentStep.EntriesName] is not JsonArray entries)
        {
            return;
        }

        var position = await _store.NextPositionAsync();

        foreach (var entry in entries.OfType<JsonObject>())
        {
            var id = entry["ItemId"]?.GetValue<string>();

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
                continue;
            }

            if (string.IsNullOrEmpty(scheme.Name) || AdminSchemeStore.ToDefinition(scheme) is null)
            {
                context.Errors.Add(S["The scheme '{0}' needs a name, four '#rrggbb' colors (Primary, Deep, Ink, Accent), and '#rrggbb' optional colors.", scheme.Id]);
                continue;
            }

            if (!AdminSchemeStore.HasValidTypography(scheme))
            {
                context.Errors.Add(S["The scheme '{0}' has an unknown font family (use 'lato', 'inter', 'source-sans-3' or 'system'), or a FontScale (0.875 to 1.125), RadiusScale (0 to 1.5) or Density (0.75 to 1.25) out of range.", scheme.Id]);
                continue;
            }

            await _store.SaveAsync(scheme);
        }
    }
}
