using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Contents.Drivers
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
                Shape("Contents__Metadata", model).Location("Detail", "Content:before"),
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

            if (contentItem.IsPublished() &&
                contentTypeDefinition.Settings.ToObject<ContentTypeSettings>().Updatable)
            {
                results.Add(Shape("Content_UpdateButton").Location("Actions:20"));
            }

            if (contentTypeDefinition.Settings.ToObject<ContentTypeSettings>().Draftable)
            {
                results.Add(Shape("Content_SaveDraftButton").Location("Actions:30"));
            }

            return Combine(results.ToArray());
        }
    }
}