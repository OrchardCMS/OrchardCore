using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants.Recipes;

public sealed class FeatureProfilesRecipeStep : RecipeDeploymentStep<FeatureProfilesRecipeStep.FeatureProfilesStepModel>
{
    private readonly FeatureProfilesManager _featureProfilesManager;

    public FeatureProfilesRecipeStep(FeatureProfilesManager featureProfilesManager)
    {
        _featureProfilesManager = featureProfilesManager;
    }

    public override string Name => "FeatureProfiles";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Feature Profiles")
            .Description("Defines tenant feature profiles.")
            .Required("name", "FeatureProfiles")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("FeatureProfiles", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .AdditionalProperties(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .Properties(
                            ("Id", new RecipeStepSchemaBuilder().TypeString()),
                            ("Name", new RecipeStepSchemaBuilder().TypeString()),
                            ("FeatureRules", new RecipeStepSchemaBuilder()
                                .TypeArray()
                                .Items(new RecipeStepSchemaBuilder()
                                    .TypeObject()
                                    .Required("Rule", "Expression")
                                    .Properties(
                                        ("Rule", new RecipeStepSchemaBuilder().TypeString()),
                                        ("Expression", new RecipeStepSchemaBuilder().TypeString()))
                                    .AdditionalProperties(true))))
                        .AdditionalProperties(true)
                        .Build())))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(FeatureProfilesStepModel model, RecipeExecutionContext context)
    {
        if (model.FeatureProfiles != null)
        {
            foreach (var property in model.FeatureProfiles)
            {
                await _featureProfilesManager.UpdateFeatureProfileAsync(property.Key, property.Value);
            }
        }
    }

    protected override async Task<FeatureProfilesStepModel> BuildExportModelAsync(RecipeExportContext context)
    {
        var featureProfilesDocument = await _featureProfilesManager.GetFeatureProfilesDocumentAsync();

        return new FeatureProfilesStepModel
        {
            FeatureProfiles = new Dictionary<string, FeatureProfile>(featureProfilesDocument.FeatureProfiles),
        };
    }

    public sealed class FeatureProfilesStepModel
    {
        public Dictionary<string, FeatureProfile> FeatureProfiles { get; set; }
    }
}
