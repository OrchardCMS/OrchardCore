using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.DisplayManagement.Views;
using Orchard.ContentManagement.MetaData;
using System.Threading.Tasks;
using Orchard.ContentManagement.MetaData.Settings;

namespace Orchard.Contents.Drivers
{
    public class ContentsDriver : ContentDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentsDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(ContentItem model)
        {
            return Combine(
                Shape("Parts_Contents_Publish", model),
                Shape("Parts_Contents_Publish_Summary", model),
                Shape("Parts_Contents_Publish_SummaryAdmin", model),
                Shape("Parts_Contents_Clone_SummaryAdmin", model)
            );
        }

        public override IDisplayResult Edit(ContentItem contentItem)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            var results = new List<IDisplayResult> { Shape("Content_SaveButton").Location("Content:after") };

            if (contentTypeDefinition.Settings.ToObject<ContentTypeSettings>().Draftable)
            {
                results.Add(Shape("Content_PublishButton").Location("Content:after"));
            }

            return Combine(results.ToArray());
        }

        public override Task<IDisplayResult> UpdateAsync(ContentItem contentItem, IUpdateModel updater)
        {
            return Task.FromResult(Edit(contentItem));
        }
    }
}