using System.Text.Json.Nodes;
using OrchardCore.Admin;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Themes.Services;

namespace OrchardCore.Themes.Recipes;

/// <summary>
/// This recipe step defines the site and admin current themes.
/// </summary>
public sealed class ThemesStep : NamedRecipeStepHandler
{
    private readonly ISiteThemeService _siteThemeService;
    private readonly IAdminThemeService _adminThemeService;

    public ThemesStep(
        ISiteThemeService siteThemeService,
        IAdminThemeService adminThemeService)
        : base("Themes")
    {
        _adminThemeService = adminThemeService;
        _siteThemeService = siteThemeService;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
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
