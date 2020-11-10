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

            foreach (var contentType in step.ContentTypes)
            {
                var newType = _contentDefinitionManager.LoadTypeDefinition(contentType.Name);
                if (newType != null)
                {
                    ReplaceContentType(newType, contentType, true);
                }
                else
                {
                    ReplaceContentType(new ContentTypeDefinition(contentType.Name, contentType.DisplayName), contentType, false);
                }
            }

            foreach (var contentPart in step.ContentParts)
            {
                var newPart = _contentDefinitionManager.LoadPartDefinition(contentPart.Name);
                if (newPart != null)
                {
                    ReplaceContentPart(newPart, contentPart, true);
                }
                else
                {
                    ReplaceContentPart(new ContentPartDefinition(contentPart.Name), contentPart, false);
                }
            }

            return Task.CompletedTask;
        }

        private void ReplaceContentType(ContentTypeDefinition type, ContentTypeDefinitionRecord record, bool exists)
        {
            if (exists)
            {
                _contentDefinitionManager.DeleteTypeDefinition(type.Name);
            }

            _contentDefinitionManager.AlterTypeDefinition(type.Name, builder =>
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

        private void ReplaceContentPart(ContentPartDefinition part, ContentPartDefinitionRecord record, bool exists)
        {
            if (exists)
            {
                _contentDefinitionManager.DeletePartDefinition(part.Name);
            }

            _contentDefinitionManager.AlterPartDefinition(part.Name, builder =>
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
