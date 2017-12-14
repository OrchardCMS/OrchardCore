using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentTypes.Editors
{
    public abstract class ContentPartDisplayDriver : DisplayDriver<ContentPartDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartEditorContext>, IContentPartDefinitionDisplayDriver
    {
        public override string GeneratePrefix(ContentPartDefinition model)
        {
            return model.Name;
        }

        public override bool CanHandleModel(ContentPartDefinition model)
        {
            return true;
        }
    }
}