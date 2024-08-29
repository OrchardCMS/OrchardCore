using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.CustomSettings.Recipes;

/// <summary>
/// This recipe step updates the site settings.
/// </summary>
public sealed class CustomSettingsStep : IRecipeStepHandler
{
    private readonly ISiteService _siteService;

    public CustomSettingsStep(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "custom-settings", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var siteSettings = await _siteService.LoadSiteSettingsAsync();

        var model = context.Step;
        foreach (var customSettings in model)
        {
            if (customSettings.Key == "name")
            {
                continue;
            }

            siteSettings.Properties[customSettings.Key] = customSettings.Value.DeepClone();
        }

        await _siteService.UpdateSiteSettingsAsync(siteSettings);
    }
}
