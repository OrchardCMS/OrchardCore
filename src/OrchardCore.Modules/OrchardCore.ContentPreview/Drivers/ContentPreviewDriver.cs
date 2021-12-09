using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentPreview.Drivers
{
    public class ContentPreviewDriver : ContentDisplayDriver
    {
        public override IDisplayResult Edit(ContentItem contentItem, IUpdateModel updater)
        {
            return Shape("ContentPreview_Button", new ContentItemViewModel(contentItem)).Location("Actions:after");
        }
    }
}
