using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.ContentTypes.RecipeSteps;

/// <summary>
/// This recipe step replaces content definition records.
/// </summary>
public sealed class ReplaceContentDefinitionStep : NamedRecipeStepHandler
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ReplaceContentDefinitionStep(IContentDefinitionManager contentDefinitionManager)
        : base("ReplaceContentDefinition")
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var step = context.Step.ToObject<ReplaceContentDefinitionStepModel>();

        // Delete existing parts first, as deleting them later will clear any imported content types using them.
        foreach (var contentPart in step.ContentParts)
        {
            await _contentDefinitionManager.DeletePartDefinitionAsync(contentPart.Name);
        }

        foreach (var contentType in step.ContentTypes)
        {
            await _contentDefinitionManager.DeleteTypeDefinitionAsync(contentType.Name);
            await AlterContentTypeAsync(contentType);
        }

        foreach (var contentPart in step.ContentParts)
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
                builder.DisplayedAs(record.DisplayName);
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

    private sealed class ReplaceContentDefinitionStepModel
    {
        public ContentTypeDefinitionRecord[] ContentTypes { get; set; } = [];
        public ContentPartDefinitionRecord[] ContentParts { get; set; } = [];
    }
}
