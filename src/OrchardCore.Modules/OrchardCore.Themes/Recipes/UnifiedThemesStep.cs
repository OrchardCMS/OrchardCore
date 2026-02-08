using OrchardCore.Recipes.Schema;
using OrchardCore.Admin;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Themes.Services;

namespace OrchardCore.Themes.Recipes;

/// <summary>
/// Unified recipe/deployment step for setting and exporting site and admin themes.
/// </summary>
public sealed class UnifiedThemesStep : RecipeDeploymentStep<UnifiedThemesStep.ThemesStepModel>
{
    private readonly ISiteThemeService _siteThemeService;
    private readonly IAdminThemeService _adminThemeService;

    public UnifiedThemesStep(
        ISiteThemeService siteThemeService,
        IAdminThemeService adminThemeService)
    {
        _siteThemeService = siteThemeService;
        _adminThemeService = adminThemeService;
    }

    /// <inheritdoc />
    public override string Name => "Themes";

    /// <inheritdoc />
    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title(Name)
            .Description("Sets the site and admin themes for the Orchard Core application.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("site", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The technical name of the site theme to activate.")),
                ("admin", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The technical name of the admin theme to activate.")))
            .AdditionalProperties(false)
            .Build();
    }

    /// <inheritdoc />
    protected override async Task ImportAsync(ThemesStepModel model, RecipeExecutionContext context)
    {
        if (!string.IsNullOrEmpty(model.Site))
        {
            await _siteThemeService.SetSiteThemeAsync(model.Site);
        }

        if (!string.IsNullOrEmpty(model.Admin))
        {
            await _adminThemeService.SetAdminThemeAsync(model.Admin);
        }
    }

    /// <inheritdoc />
    protected override async Task<ThemesStepModel> BuildExportModelAsync(RecipeExportContext context)
    {
        return new ThemesStepModel
        {
            Site = await _siteThemeService.GetSiteThemeNameAsync(),
            Admin = await _adminThemeService.GetAdminThemeNameAsync(),
        };
    }

    /// <summary>
    /// Model for the Themes step data.
    /// </summary>
    public sealed class ThemesStepModel
    {
        /// <summary>
        /// Gets or sets the step name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the site theme name.
        /// </summary>
        public string Site { get; set; }

        /// <summary>
        /// Gets or sets the admin theme name.
        /// </summary>
        public string Admin { get; set; }
    }
}
