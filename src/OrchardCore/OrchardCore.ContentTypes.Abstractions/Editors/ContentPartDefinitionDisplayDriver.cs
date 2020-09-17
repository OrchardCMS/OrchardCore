using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentTypes.Editors
{
    public abstract class ContentPartDefinitionDisplayDriver : DisplayDriver<ContentPartDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartEditorContext>, IContentPartDefinitionDisplayDriver
    {
        public override bool CanHandleModel(ContentPartDefinition model)
        {
            return true;
        }
    }
}
