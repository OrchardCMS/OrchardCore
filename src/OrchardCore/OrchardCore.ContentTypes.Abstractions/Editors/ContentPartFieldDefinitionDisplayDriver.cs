using System;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentTypes.Editors
{
    /// <summary>
    /// A concrete implementation of <see cref="ContentPartFieldDefinitionDisplayDriver"/> provides a driver for all field definitions.
    /// </summary>
    public abstract class ContentPartFieldDefinitionDisplayDriver : DisplayDriver<ContentPartFieldDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartFieldEditorContext>, IContentPartFieldDefinitionDisplayDriver
    {
        protected override void BuildPrefix(ContentPartFieldDefinition model, string htmlFieldPrefix)
        {
            Prefix = $"{model.PartDefinition.Name}.{model.Name}";

            if (!String.IsNullOrEmpty(htmlFieldPrefix))
            {
                Prefix = htmlFieldPrefix + "." + Prefix;
            }

            // Prefix any driver with a unique name
            Prefix += "." + GetType().Name;
        }

        public override bool CanHandleModel(ContentPartFieldDefinition model)
        {
            // This drivers applies to all fields.
            return true;
        }
    }

    /// <summary>
    /// A concrete implementation of <see cref="ContentPartFieldDefinitionDisplayDriver&lt;TField&gt;"/> provides a driver for field definitions
    /// of the type <c>TField</c>.
    /// </summary>
    public abstract class ContentPartFieldDefinitionDisplayDriver<TField> : ContentPartFieldDefinitionDisplayDriver where TField : ContentField
    {
        public override bool CanHandleModel(ContentPartFieldDefinition model)
        {
            return string.Equals(typeof(TField).Name, model.FieldDefinition.Name);
        }
    }
}
