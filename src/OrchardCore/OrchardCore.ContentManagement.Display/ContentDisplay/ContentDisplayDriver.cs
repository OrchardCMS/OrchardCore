using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentManagement.Display.ContentDisplay;

public abstract class ContentDisplayDriver : DisplayDriver<ContentItem, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>, IContentDisplayDriver
{
}
