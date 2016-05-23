using Orchard.ContentManagement.MetaData.Models;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentTypes.Editors
{
    public abstract class ContentTypePartDisplayDriver : DisplayDriver<ContentTypePartDefinition, BuildDisplayContext, BuildEditorContext, UpdateTypePartEditorContext>, IContentTypePartDefinitionDisplayDriver
    {
        public override string GeneratePrefix(ContentTypePartDefinition model)
        {
            return $"{model.ContentTypeDefinition.Name}.{model.PartDefinition.Name}";
        }
    }
}