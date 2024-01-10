using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.ContentTypes.RecipeSteps
{
    /// <summary>
    /// This recipe step creates content definitions.
    /// </summary>
    public class ContentDefinitionStep : IRecipeStepHandler
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentDefinitionStep(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "ContentDefinition", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var step = context.Step.ToObject<ContentDefinitionStepModel>();

            foreach (var contentType in step.ContentTypes)
            {
                var newType = await _contentDefinitionManager.LoadTypeDefinitionAsync(contentType.Name)
                    ?? new ContentTypeDefinition(contentType.Name, contentType.DisplayName);

                await UpdateContentTypeAsync(newType, contentType);
            }

            foreach (var contentPart in step.ContentParts)
            {
                var newPart = await _contentDefinitionManager.LoadPartDefinitionAsync(contentPart.Name)
                    ?? new ContentPartDefinition(contentPart.Name);

                await UpdateContentPartAsync(newPart, contentPart);
            }
        }

        private Task UpdateContentTypeAsync(ContentTypeDefinition type, ContentTypeDefinitionRecord record)
            => _contentDefinitionManager.AlterTypeDefinitionAsync(type.Name, builder =>
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

        private Task UpdateContentPartAsync(ContentPartDefinition part, ContentPartDefinitionRecord record)
            => _contentDefinitionManager.AlterPartDefinitionAsync(part.Name, builder =>
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

        private class ContentDefinitionStepModel
        {
            public ContentTypeDefinitionRecord[] ContentTypes { get; set; } = Array.Empty<ContentTypeDefinitionRecord>();
            public ContentPartDefinitionRecord[] ContentParts { get; set; } = Array.Empty<ContentPartDefinitionRecord>();
        }
    }
}
