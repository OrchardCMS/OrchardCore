using OrchardCore.Recipes.Schema;
using OrchardCore.Entities;
using OrchardCore.GitHub.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Recipes;

public sealed class GitHubAuthenticationSettingsRecipeStep : RecipeImportStep<GitHubLoginSettingsStepModel>
{
    private readonly ISiteService _siteService;

    public GitHubAuthenticationSettingsRecipeStep(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override string Name => nameof(GitHubAuthenticationSettings);

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("GitHub Authentication Settings")
            .Description("Imports GitHub authentication settings.")
            .Required("name", "Files")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                 ("ClientID", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The Client ID for GitHub authentication.")),
                 ("ClientSecret", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The Client Secret for GitHub authentication.")),
                 ("CallbackPath", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The Callback Path for GitHub authentication.")))
            .AdditionalProperties(false)
            .Build();
    }

    protected override async Task ImportAsync(GitHubLoginSettingsStepModel model, RecipeExecutionContext context)
    {
        var site = await _siteService.LoadSiteSettingsAsync();

        site.Alter<GitHubAuthenticationSettings>(settings =>
        {
            settings.ClientID = model.ConsumerKey;
            settings.ClientSecret = model.ConsumerSecret;
            settings.CallbackPath = model.CallbackPath;
        });

        await _siteService.UpdateSiteSettingsAsync(site);
    }
}
