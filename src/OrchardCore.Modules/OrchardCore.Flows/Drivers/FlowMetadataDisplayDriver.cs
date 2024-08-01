using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.Drivers
{
    public class FlowMetadataDisplayDriver : ContentSyncDisplayDriver
    {
        public override IDisplayResult Edit(ContentItem model, BuildEditorContext context)
        {
            var flowMetadata = model.As<FlowMetadata>();

            if (flowMetadata == null)
            {
                return null;
            }

            return Initialize<FlowMetadata>("FlowMetadata_Edit", metadata =>
            {
                metadata.Alignment = flowMetadata.Alignment;
                metadata.Size = flowMetadata.Size;
            }).Location("Footer");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItem contentItem, UpdateEditorContext context)
        {
            var flowMetadata = contentItem.As<FlowMetadata>();

            if (flowMetadata == null)
            {
                return null;
            }

            await contentItem.AlterAsync<FlowMetadata>(model => context.Updater.TryUpdateModelAsync(model, Prefix));

            return await EditAsync(contentItem, context);
        }
    }
}
