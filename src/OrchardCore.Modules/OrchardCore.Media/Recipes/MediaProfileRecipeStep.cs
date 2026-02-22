using OrchardCore.Recipes.Schema;
using OrchardCore.Media.Models;
using OrchardCore.Media.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Media.Recipes;

public sealed class MediaProfileRecipeStep : RecipeImportStep<MediaProfileRecipeStep.MediaProfileStepModel>
{
    private readonly MediaProfilesManager _mediaProfilesManager;

    public MediaProfileRecipeStep(MediaProfilesManager mediaProfilesManager)
    {
        _mediaProfilesManager = mediaProfilesManager;
    }

    public override string Name => "MediaProfiles";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Media Profiles")
            .Description("Creates or updates media processing profiles.")
            .Required("name", "MediaProfiles")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("MediaProfiles", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .AdditionalProperties(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .Properties(
                            ("Hint", new RecipeStepSchemaBuilder()
                                .TypeString()),
                            ("Width", new RecipeStepSchemaBuilder()
                                .TypeInteger()),
                            ("Height", new RecipeStepSchemaBuilder()
                                .TypeInteger()),
                            ("Mode", new RecipeStepSchemaBuilder()
                                .TypeString()
                                .Description("The image resize mode.")),
                            ("Format", new RecipeStepSchemaBuilder()
                                .TypeString()
                                .Description("The output image format.")),
                            ("Quality", new RecipeStepSchemaBuilder()
                                .TypeInteger()),
                            ("BackgroundColor", new RecipeStepSchemaBuilder()
                                .TypeString()))
                        .AdditionalProperties(true)
                        .Build())
                    .Description("A dictionary keyed by profile name.")))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(MediaProfileStepModel model, RecipeExecutionContext context)
    {
        foreach (var mediaProfile in model.MediaProfiles)
        {
            await _mediaProfilesManager.UpdateMediaProfileAsync(mediaProfile.Key, mediaProfile.Value);
        }
    }

    public sealed class MediaProfileStepModel
    {
        public Dictionary<string, MediaProfile> MediaProfiles { get; set; }
    }
}
