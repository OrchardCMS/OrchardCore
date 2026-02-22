using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.ContentTypes.RecipeSteps;

public sealed class DeleteContentDefinitionRecipeStep : RecipeImportStep<DeleteContentDefinitionRecipeStep.DeleteContentDefinitionStepModel>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public DeleteContentDefinitionRecipeStep(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public override string Name => "DeleteContentDefinition";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Delete Content Definition")
            .Description("Deletes content types/parts by name.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("ContentTypes", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder().TypeString())),
                ("ContentParts", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder().TypeString())))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(DeleteContentDefinitionStepModel model, RecipeExecutionContext context)
    {
        foreach (var contentType in model.ContentTypes)
        {
            await _contentDefinitionManager.DeleteTypeDefinitionAsync(contentType);
        }

        foreach (var contentPart in model.ContentParts)
        {
            await _contentDefinitionManager.DeletePartDefinitionAsync(contentPart);
        }
    }

    public sealed class DeleteContentDefinitionStepModel
    {
        public string[] ContentTypes { get; set; } = [];
        public string[] ContentParts { get; set; } = [];
    }
}
