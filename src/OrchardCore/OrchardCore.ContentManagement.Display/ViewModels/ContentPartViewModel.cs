using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentManagement.Display.ViewModels
{
    public class ContentPartViewModel : ShapeViewModel
    {
        public ContentPartViewModel()
        {
        }

        public ContentPartViewModel(ContentPart contentPart)
        {
            ContentPart = contentPart;
        }

        public ContentPart ContentPart { get; set; }
    }
}
