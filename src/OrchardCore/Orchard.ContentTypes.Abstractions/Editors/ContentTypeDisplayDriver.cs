using System;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentTypes.Editors
{
    public abstract class ContentTypeDisplayDriver : DisplayDriver<ContentTypeDefinition, BuildDisplayContext, BuildEditorContext, UpdateTypeEditorContext>, IContentTypeDefinitionDisplayDriver
    {
        public override string GeneratePrefix(ContentTypeDefinition model)
        {
            return model.Name;
        }

        public override bool CanHandleModel(ContentTypeDefinition model)
        {
            return true;
        }
    }
}