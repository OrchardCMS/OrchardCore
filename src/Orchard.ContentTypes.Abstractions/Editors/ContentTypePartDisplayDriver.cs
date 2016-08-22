using System;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentTypes.Editors
{
    public abstract class ContentTypePartDisplayDriver : DisplayDriver<ContentTypePartDefinition, BuildDisplayContext, BuildEditorContext, UpdateTypePartEditorContext>, IContentTypePartDefinitionDisplayDriver
    {
        public override string GeneratePrefix(ContentTypePartDefinition model)
        {
            return $"{model.ContentTypeDefinition.Name}.{model.PartDefinition.Name}";
        }

        public override bool CanHandleModel(ContentTypePartDefinition model)
        {
            return true;
        }
    }
}