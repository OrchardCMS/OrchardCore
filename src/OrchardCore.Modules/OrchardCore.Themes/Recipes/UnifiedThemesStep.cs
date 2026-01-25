using Json.Schema;
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
    public override string DisplayName => "Themes";

    /// <inheritdoc />
    public override string Description => "Sets the site and admin themes for the Orchard Core application.";

    /// <inheritdoc />
    public override string Category => "Configuration";

    /// <inheritdoc />
    protected override JsonSchema BuildSchema()
    {
        return new JsonSchemaBuilder()
            .Schema(MetaSchemas.Draft202012Id)
            .Type(SchemaValueType.Object)
            .Title(Name)
            .Description(Description)
            .Required("name")
            .Properties(
                ("name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("site", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The technical name of the site theme to activate.")),
                ("admin", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The technical name of the admin theme to activate.")))
            .AdditionalProperties(JsonSchema.False)
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
