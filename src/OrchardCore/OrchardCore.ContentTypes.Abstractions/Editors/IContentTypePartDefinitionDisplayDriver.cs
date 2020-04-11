using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentTypes.Editors
{
    public interface IContentTypePartDefinitionDisplayDriver : IDisplayDriver<ContentTypePartDefinition, BuildDisplayContext, BuildEditorContext, UpdateTypePartEditorContext>
    {
    }
}
