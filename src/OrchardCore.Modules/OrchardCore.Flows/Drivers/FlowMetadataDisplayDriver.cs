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
        if (!model.TryGet<FlowMetadata>(out var flowMetadata))
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
        if (!contentItem.TryGet<FlowMetadata>(out _))
        {
            return null;
        }

        await contentItem.AlterAsync<FlowMetadata>(model => context.Updater.TryUpdateModelAsync(model, Prefix));

        return Edit(contentItem, context);
    }
}
