using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Drivers
{
    public class ContentsDriver : ContentDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentsDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(ContentItem model, IUpdateModel updater)
        {
            return Combine(
                Shape("ContentsMetadata", model).Location("Detail", "Content:before"),
                Shape("Contents_SummaryAdmin__Tags", model).Location("SummaryAdmin", "Meta:10"),
                Shape("Contents_SummaryAdmin__Meta", model).Location("SummaryAdmin", "Meta:20"),
                Shape("Contents_SummaryAdmin__Button__Edit", model).Location("SummaryAdmin", "Actions:10"),
                Shape("Contents_SummaryAdmin__Button__Actions", model).Location("SummaryAdmin", "Actions:20")
            );
        }

        public override IDisplayResult Edit(ContentItem contentItem, IUpdateModel updater)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            var results = new List<IDisplayResult>
            {
                Shape("Content_PublishButton").Location("Actions:10"),
            };

            if (contentTypeDefinition.Settings.ToObject<ContentTypeSettings>().Draftable)
            {
                results.Add(Shape("Content_SaveDraftButton").Location("Actions:20"));
            }

            return Combine(results.ToArray());
        }
    }
}