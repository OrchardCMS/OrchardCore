using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.ContentTypes.RecipeSteps
{
    /// <summary>
    /// This recipe step replaces content definition records.
    /// </summary>
    public class ReplaceContentDefinitionStep : IRecipeStepHandler
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ReplaceContentDefinitionStep(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "ReplaceContentDefinition", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var step = context.Step.ToObject<ReplaceContentDefinitionStepModel>();

            // Delete existing parts first, as deleting them later will clear any imported content types using them.
            foreach (var contentPart in step.ContentParts)
            {
                _contentDefinitionManager.DeletePartDefinition(contentPart.Name);
            }

            foreach (var contentType in step.ContentTypes)
            {
                _contentDefinitionManager.DeleteTypeDefinition(contentType.Name);
                AlterContentType(contentType);
            }

            foreach (var contentPart in step.ContentParts)
            {
                AlterContentPart(contentPart);
            }

            return Task.CompletedTask;
        }

        private void AlterContentType(ContentTypeDefinitionRecord record)
        {
            _contentDefinitionManager.AlterTypeDefinition(record.Name, builder =>
            {
                if (!String.IsNullOrEmpty(record.DisplayName))
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

        private void AlterContentPart(ContentPartDefinitionRecord record)
        {
            _contentDefinitionManager.AlterPartDefinition(record.Name, builder =>
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

        private class ReplaceContentDefinitionStepModel
        {
            public ContentTypeDefinitionRecord[] ContentTypes { get; set; } = Array.Empty<ContentTypeDefinitionRecord>();
            public ContentPartDefinitionRecord[] ContentParts { get; set; } = Array.Empty<ContentPartDefinitionRecord>();
        }
    }
}
