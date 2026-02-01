using Json.Schema;
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
        return new JsonSchemaBuilder()
            .Schema(MetaSchemas.Draft202012Id)
            .Type(SchemaValueType.Object)
            .Title("GitHub Authentication Settings")
            .Description("Imports GitHub authentication settings.")
            .Required("name", "Files")
            .Properties(
                ("name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                 ("ClientID", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The Client ID for GitHub authentication.")),
                 ("ClientSecret", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("The Client Secret for GitHub authentication.")),
                 ("CallbackPath", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
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
