using System.Text.Json.Nodes;
using OrchardCore.Admin;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Themes.Services;

namespace OrchardCore.Themes.Recipes;

/// <summary>
/// This recipe step defines the site and admin current themes.
/// </summary>
/// <remarks>
/// This class is obsolete. Implement <see cref="IRecipeDeploymentStep"/> instead.
/// </remarks>
[Obsolete($"Implement {nameof(IRecipeDeploymentStep)} instead. This class will be removed in a future version.", false)]
#pragma warning disable CS0618 // Type or member is obsolete - required for backwards compatibility
public sealed class ThemesStep : NamedRecipeStepHandler
#pragma warning restore CS0618
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

/// <remarks>
/// This class is obsolete. Use your own model class instead.
/// </remarks>
[Obsolete("Use your own model class instead. This class will be removed in a future version.", false)]
public sealed class ThemeStepModel
{
    public string Site { get; set; }
    public string Admin { get; set; }
}
