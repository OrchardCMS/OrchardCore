using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.ContentTypes.RecipeSteps;

public sealed class ReplaceContentDefinitionRecipeStep : RecipeImportStep<ReplaceContentDefinitionRecipeStep.ReplaceContentDefinitionStepModel>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ReplaceContentDefinitionRecipeStep(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public override string Name => "ReplaceContentDefinition";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Replace Content Definition")
            .Description("Replaces content type/part definitions entirely.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("ContentTypes", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .AdditionalProperties(true))),
                ("ContentParts", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .AdditionalProperties(true))))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(ReplaceContentDefinitionStepModel model, RecipeExecutionContext context)
    {
        foreach (var contentPart in model.ContentParts)
        {
            await _contentDefinitionManager.DeletePartDefinitionAsync(contentPart.Name);
        }

        foreach (var contentType in model.ContentTypes)
        {
            await _contentDefinitionManager.DeleteTypeDefinitionAsync(contentType.Name);
            await AlterContentTypeAsync(contentType);
        }

        foreach (var contentPart in model.ContentParts)
        {
            await AlterContentPartAsync(contentPart);
        }
    }

    private async Task AlterContentTypeAsync(ContentTypeDefinitionRecord record)
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync(record.Name, builder =>
        {
            if (!string.IsNullOrEmpty(record.DisplayName))
            {
                builder.WithDisplayName(record.DisplayName);
                builder.MergeSettings(record.Settings);
            }

            foreach (var part in record.ContentTypePartDefinitionRecords)
            {
                builder.WithPart(part.Name, part.PartName, partBuilder => partBuilder.MergeSettings(part.Settings));
            }
        });
    }

    private async Task AlterContentPartAsync(ContentPartDefinitionRecord record)
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(record.Name, builder =>
        {
            builder.MergeSettings(record.Settings);

            foreach (var field in record.ContentPartFieldDefinitionRecords)
            {
                builder.WithField(field.Name, fieldBuilder =>
                {
                    fieldBuilder.OfType(field.FieldName);
                    fieldBuilder.MergeSettings(field.Settings);
                });
            }
        });
    }

    public sealed class ReplaceContentDefinitionStepModel
    {
        public ContentTypeDefinitionRecord[] ContentTypes { get; set; } = [];
        public ContentPartDefinitionRecord[] ContentParts { get; set; } = [];
    }
}
