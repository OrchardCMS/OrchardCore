using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Layers.ViewModels
{
    public class WidgetWrapper : ShapeViewModel
    {
        public WidgetWrapper() : base("Widget_Wrapper")
        {
        }

        public ContentItem Widget { get; set; }
        public IShape Content { get; set; }
    }
}
