using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.Drivers;

public sealed class FlowMetadataDisplayDriver : ContentDisplayDriver
{
    public override IDisplayResult Edit(ContentItem model, BuildEditorContext context)
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

    public override async Task<IDisplayResult> UpdateAsync(ContentItem contentItem, UpdateEditorContext context)
    {
        var flowMetadata = contentItem.As<FlowMetadata>();

        if (flowMetadata == null)
        {
            return null;
        }

        await contentItem.AlterAsync<FlowMetadata>(model => context.Updater.TryUpdateModelAsync(model, Prefix));

        return Edit(contentItem, context);
    }
}
