using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Themes.Recipes;

/// <summary>
/// Checks the themes that a <see cref="ThemesStep"/> would select, which it stores without looking at them.
/// </summary>
public sealed class ThemesStepValidator : IRecipeStepValidator
{
    private readonly IShellFeaturesManager _shellFeaturesManager;

    internal readonly IStringLocalizer S;

    public ThemesStepValidator(IShellFeaturesManager shellFeaturesManager, IStringLocalizer<ThemesStepValidator> stringLocalizer)
    {
        _shellFeaturesManager = shellFeaturesManager;
        S = stringLocalizer;
    }

    /// <inheritdoc />
    public string StepName => "Themes";

    /// <inheritdoc />
    public async Task ValidateAsync(RecipeValidationContext context)
    {
        var step = context.Step.ToObject<ThemeStepModel>();

        await ValidateThemeAsync(step.Site, context);
        await ValidateThemeAsync(step.Admin, context);
    }

    private async Task ValidateThemeAsync(string id, RecipeValidationContext context)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        if (RecipeValidationContext.IsScript(id))
        {
            context.AddWarning("theme-not-evaluated", S["The theme '{0}' is a script, which only the execution evaluates.", id]);

            return;
        }

        var available = await _shellFeaturesManager.GetAvailableFeaturesAsync();
        if (!available.Any(feature => feature.Id == id && feature.IsTheme()))
        {
            context.AddError("theme-not-found", S["The theme '{0}' doesn't exist or isn't available on this tenant. The step would select it anyway.", id]);

            return;
        }

        var enabled = await _shellFeaturesManager.GetEnabledFeaturesAsync();
        if (!context.FeaturesMayChange && !context.PendingFeatures.Contains(id) && !enabled.Any(feature => feature.Id == id))
        {
            context.AddWarning("theme-not-enabled", S["The theme '{0}' isn't enabled, and no earlier step enables it.", id]);
        }
    }
}
