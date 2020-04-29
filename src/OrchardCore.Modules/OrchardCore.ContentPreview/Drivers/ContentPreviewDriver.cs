using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentPreview.Drivers
{
    public class ContentPreviewDriver : ContentDisplayDriver
    {
        public override Task<IDisplayResult> EditAsync(ContentItem model, BuildEditorContext context)
        {
            if (context.Updater.IsContained)
            {
                return null;
            }

            return Task.FromResult((IDisplayResult)Shape("ContentPreview_Button", new ContentItemViewModel(model)).Location("Actions:after"));
        }
    }
}
