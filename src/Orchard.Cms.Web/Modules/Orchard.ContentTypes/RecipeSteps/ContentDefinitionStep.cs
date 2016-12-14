using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentManagement.Metadata.Records;
using Orchard.ContentManagement.MetaData;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.ContentTypes.RecipeSteps
{
    public class ContentDefinitionStep : RecipeExecutionStep
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentDefinitionStep(
            IContentDefinitionManager contentDefinitionManager,
            ILogger<ContentDefinitionStep> logger,
            IStringLocalizer<ContentDefinitionStep> localizer) : base(logger, localizer)
        {

            _contentDefinitionManager = contentDefinitionManager;
        }

        public override string Name => "ContentDefinition";

        public override Task ExecuteAsync(RecipeExecutionContext context)
        {
            var step = context.RecipeStep.Step.ToObject<ContentDefinitionStepData>();

            foreach (var contentType in step.ContentTypes)
            {
                var newType = _contentDefinitionManager.GetTypeDefinition(contentType.Name)
                    ?? new ContentTypeDefinition(contentType.Name, contentType.DisplayName);

                UpdateContentType(newType, contentType);
            }

            foreach (var contentPart in step.ContentParts)
            {
                var newPart = _contentDefinitionManager.GetPartDefinition(contentPart.Name)
                    ?? new ContentPartDefinition(contentPart.Name);

                UpdateContentPart(newPart, contentPart);
            }

            return Task.CompletedTask;
        }

        private void UpdateContentType(ContentTypeDefinition type, ContentTypeDefinitionRecord record)
        {
            _contentDefinitionManager.AlterTypeDefinition(type.Name, builder =>
            {
                if (!String.IsNullOrEmpty(record.DisplayName))
                {
                    builder.DisplayedAs(record.DisplayName);
                    builder.MergeSettings(record.Settings);
                }

                foreach(var part in record.ContentTypePartDefinitionRecords)
                {
                    builder.WithPart(part.Name, part.PartName, partBuilder => partBuilder.MergeSettings(part.Settings));
                }
            });

        }

        private void UpdateContentPart(ContentPartDefinition part, ContentPartDefinitionRecord record)
        {
            _contentDefinitionManager.AlterPartDefinition(part.Name, builder =>
            {
                builder.MergeSettings(record.Settings);

                foreach (var field in record.ContentPartFieldDefinitionRecords)
                {
                    builder.WithField(field.Name, fieldBuilder =>
                    {
                        fieldBuilder.MergeSettings(part.Settings);
                    });
                }
            });
        }

        private class ContentDefinitionStepData
        {
            public ContentTypeDefinitionRecord[] ContentTypes { get; set; } = Array.Empty<ContentTypeDefinitionRecord>();
            public ContentPartDefinitionRecord[] ContentParts { get; set; } = Array.Empty<ContentPartDefinitionRecord>();
        }
    }
}
