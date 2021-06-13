using System;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentTypes.Editors
{
    public abstract class ContentTypePartDefinitionDisplayDriver : DisplayDriver<ContentTypePartDefinition, BuildDisplayContext, BuildEditorContext, UpdateTypePartEditorContext>, IContentTypePartDefinitionDisplayDriver
    {
        protected override void BuildPrefix(ContentTypePartDefinition model, string htmlFielPrefix)
        {
            Prefix = $"{model.ContentTypeDefinition.Name}.{model.PartDefinition.Name}";

            if (!String.IsNullOrEmpty(htmlFielPrefix))
            {
                Prefix = htmlFielPrefix + "." + Prefix;
            }

            // Prefix any driver with a unique name
            Prefix += "." + GetType().Name;
        }

        public override bool CanHandleModel(ContentTypePartDefinition model)
        {
            return true;
        }
    }

    /// <summary>
    /// A concrete implementation of <see cref="ContentTypePartDefinitionDisplayDriver{TPart}"/> provides a driver for part definitions
    /// of the type <c>TPart</c>.
    /// </summary>
    public abstract class ContentTypePartDefinitionDisplayDriver<TPart> : ContentTypePartDefinitionDisplayDriver where TPart : ContentPart
    {
        public override bool CanHandleModel(ContentTypePartDefinition model)
        {
            return string.Equals(typeof(TPart).Name, model.PartDefinition.Name);
        }
    }
}
