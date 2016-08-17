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
                Shape("Parts_Contents_Publish", model),
                Shape("Parts_Contents_Publish_Summary", model),
                Shape("Parts_Contents_Publish_SummaryAdmin", model)
                    .Location("SummaryAdmin", "Actions:5"),
                Shape("Parts_Contents_Clone_SummaryAdmin", model)
                    .Location("SummaryAdmin", "Actions:6")
            );
        }

        public override IDisplayResult Edit(ContentItem contentItem, IUpdateModel updater)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            var results = new List<IDisplayResult> { Shape("Content_SaveButton").Location("Actions:0") };

            if (contentTypeDefinition.Settings.ToObject<ContentTypeSettings>().Draftable)
            {
                results.Add(Shape("Content_PublishButton").Location("Actions:5"));
            }

            return Combine(results.ToArray());
        }
    }
}