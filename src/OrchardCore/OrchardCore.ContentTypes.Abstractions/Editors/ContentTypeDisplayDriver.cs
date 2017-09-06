using System;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentTypes.Editors
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