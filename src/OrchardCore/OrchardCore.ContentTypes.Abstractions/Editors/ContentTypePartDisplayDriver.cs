using System;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentTypes.Editors
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