using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.ContentTypes.RecipeSteps;

/// <summary>
/// This recipe step creates content definitions.
/// </summary>
public sealed class ContentDefinitionStep : NamedRecipeStepHandler
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    internal readonly IStringLocalizer S;

    public ContentDefinitionStep(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<ContentDefinitionStep> stringLocalizer)
         : base("ContentDefinition")
    {
        _contentDefinitionManager = contentDefinitionManager;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var step = context.Step.ToObject<ContentDefinitionStepModel>();

        foreach (var contentType in step.ContentTypes)
        {
            var newType = await _contentDefinitionManager.LoadTypeDefinitionAsync(contentType.Name)
                ?? new ContentTypeDefinition(contentType.Name, contentType.DisplayName);

            await UpdateContentTypeAsync(newType, contentType, context);
        }

        foreach (var contentPart in step.ContentParts)
        {
            var newPart = await _contentDefinitionManager.LoadPartDefinitionAsync(contentPart.Name)
                ?? new ContentPartDefinition(contentPart.Name);

            await UpdateContentPartAsync(newPart, contentPart, context);
        }
    }

    private Task UpdateContentTypeAsync(ContentTypeDefinition type, ContentTypeDefinitionRecord record, RecipeExecutionContext context)
    {
        return _contentDefinitionManager.AlterTypeDefinitionAsync(type.Name, builder =>
        {
            if (!string.IsNullOrEmpty(record.DisplayName))
            {
                builder.DisplayedAs(record.DisplayName);
                builder.MergeSettings(record.Settings);
            }

            foreach (var part in record.ContentTypePartDefinitionRecords)
            {
                if (string.IsNullOrEmpty(part.PartName))
                {
                    context.Errors.Add(S["Unable to add content-part to the '{0}' content-type. The part name cannot be null or empty.", type.Name]);

                    continue;
                }

                builder.WithPart(part.Name, part.PartName, partBuilder => partBuilder.MergeSettings(part.Settings));
            }
        });
    }

    private Task UpdateContentPartAsync(ContentPartDefinition part, ContentPartDefinitionRecord record, RecipeExecutionContext context)
    {
        return _contentDefinitionManager.AlterPartDefinitionAsync(part.Name, builder =>
        {
            builder.MergeSettings(record.Settings);

            foreach (var field in record.ContentPartFieldDefinitionRecords)
            {
                if (string.IsNullOrEmpty(field.Name))
                {
                    context.Errors.Add(S["Unable to add content-field to the '{0}' content-part. The part name cannot be null or empty.", part.Name]);

                    continue;
                }

                builder.WithField(field.Name, fieldBuilder =>
                {
                    fieldBuilder.OfType(field.FieldName);
                    fieldBuilder.MergeSettings(field.Settings);
                });
            }
        });
    }

    private sealed class ContentDefinitionStepModel
    {
        public ContentTypeDefinitionRecord[] ContentTypes { get; set; } = [];

        public ContentPartDefinitionRecord[] ContentParts { get; set; } = [];
    }
}
