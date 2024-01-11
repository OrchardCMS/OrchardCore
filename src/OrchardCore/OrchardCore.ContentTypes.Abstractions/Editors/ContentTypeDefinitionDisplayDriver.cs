using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentTypes.Editors
{
    public abstract class ContentTypeDefinitionDisplayDriver : DisplayDriver<ContentTypeDefinition, BuildDisplayContext, BuildEditorContext, UpdateTypeEditorContext>, IContentTypeDefinitionDisplayDriver
    {
        public override bool CanHandleModel(ContentTypeDefinition model)
        {
            return true;
        }
    }
}
