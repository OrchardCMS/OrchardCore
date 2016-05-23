using Orchard.ContentManagement.MetaData.Models;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentTypes.Editors
{
    public abstract class ContentPartFieldDisplayDriver : DisplayDriver<ContentPartFieldDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartFieldEditorContext>, IContentPartFieldDefinitionDisplayDriver
    {
        public override string GeneratePrefix(ContentPartFieldDefinition model)
        {
            return $"{model.PartDefinition.Name}.{model.Name}";
        }
    }
}