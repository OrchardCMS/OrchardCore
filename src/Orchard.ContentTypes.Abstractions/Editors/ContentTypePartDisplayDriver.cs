using System;
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

        public override bool CanHandleModel(ContentTypePartDefinition model)
        {
            return true;
        }
    }

    public abstract class ContentTypePartDisplayDriver<TPart> : ContentTypePartDisplayDriver
    {
        public override bool CanHandleModel(ContentTypePartDefinition model)
        {
            return String.Equals("ListPart", model.PartDefinition.Name, StringComparison.Ordinal);
        }
    }
}