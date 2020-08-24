using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentManagement.Display.ViewModels
{
    public class ContentItemComponentModel : ShapeViewModel, IShapeComponent
    {
        // This can actually be made without the shape factor. We do that for the WidgetWrapper for example
        // But you don't get the creating events.
        public ContentItemComponentModel() : this(null)
        {

        }

        public ContentItemComponentModel(ContentItem contentItem) : base("ContentItem")
        {
            ContentItem = contentItem;
            // TODO should come from shape factory, or be passed into shape factory.
            Metadata.OnDisplaying(context => context.Shape.Metadata.Alternates.Add($"ContentItem__{ContentItem.ContentType}"));

            // Metadata.Alternates.Add($"ContentItem__{contentItem.ContentType}");
        }

        public ContentItem ContentItem { get; set; }
    }
}
