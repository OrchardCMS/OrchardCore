using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public abstract class ContentDisplayDriver : DisplayDriver<ContentItem, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>, IContentDisplayDriver
    {
    }
}
