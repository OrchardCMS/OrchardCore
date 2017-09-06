using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public interface IContentDisplayDriver : IDisplayDriver<ContentItem, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>
    {
    }
}
