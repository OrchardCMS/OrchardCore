using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.Drivers
{
    public class FlowMetadataDisplayDriver : ContentDisplayDriver
    {
        public override IDisplayResult Edit(ContentItem model, IUpdateModel updater)
        {
            var flowMetadata = model.As<FlowMetadata>();

            if (flowMetadata == null)
            {
                return null;
            }

            return Initialize<FlowMetadata>("FlowMetadata_Edit", m =>
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
