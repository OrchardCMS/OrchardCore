using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Flows.Models;

namespace Orchard.Flows.Drivers
{
    public class FlowMetadataDisplay : ContentDisplayDriver
    {
        public override IDisplayResult Edit(ContentItem model, IUpdateModel updater)
        {
            var flowMetadata = model.As<FlowMetadata>();

            if (flowMetadata == null)
            {
                return null;
            }

            return Shape<FlowMetadata>("FlowMetadata_Edit", m =>
            {
                m.Alignment = flowMetadata.Alignment;
                m.Size = flowMetadata.Size;
            }).Location("Footer");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItem contentItem, IUpdateModel updater)
        {
            var flowMetadata = contentItem.As<FlowMetadata>();

            if (flowMetadata == null)
            {
                return null;
            }

            await contentItem.AlterAsync<FlowMetadata>(model => updater.TryUpdateModelAsync(model, Prefix));

            return Edit(contentItem, updater);
        }
    }
}
