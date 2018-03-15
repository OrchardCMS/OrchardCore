using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentManagement.Display.ViewModels
{
    public class ContentItemViewModel : ShapeViewModel
    {
        public ContentItemViewModel()
        {
        }

        public ContentItemViewModel(ContentItem contentItem)
        {
            ContentItem = contentItem;
        }

        public ContentItem ContentItem { get; set; }
    }
}
