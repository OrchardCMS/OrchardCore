using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentPreview.Drivers;

public sealed class ContentPreviewDriver : ContentDisplayDriver
{
    public override IDisplayResult Edit(ContentItem contentItem, BuildEditorContext context)
    {
        return Shape("ContentPreview_Button", new ContentItemViewModel(contentItem))
            .Location("Actions:after");
    }
}
