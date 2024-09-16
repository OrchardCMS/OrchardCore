using System.Text.Json.Nodes;
using OrchardCore.Admin;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Themes.Services;

namespace OrchardCore.Themes.Recipes;

/// <summary>
/// This recipe step defines the site and admin current themes.
/// </summary>
public sealed class ThemesStep : IRecipeStepHandler
{
    private readonly ISiteThemeService _siteThemeService;
    private readonly IAdminThemeService _adminThemeService;

    public ThemesStep(
        ISiteThemeService siteThemeService,
        IAdminThemeService adminThemeService)
    {
        _adminThemeService = adminThemeService;
        _siteThemeService = siteThemeService;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "Themes", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var model = context.Step.ToObject<ThemeStepModel>();

        if (!string.IsNullOrEmpty(model.Site))
        {
            await _siteThemeService.SetSiteThemeAsync(model.Site);
        }

        if (!string.IsNullOrEmpty(model.Admin))
        {
            await _adminThemeService.SetAdminThemeAsync(model.Admin);
        }
    }
}

public sealed class ThemeStepModel
{
    public string Site { get; set; }
    public string Admin { get; set; }
}
