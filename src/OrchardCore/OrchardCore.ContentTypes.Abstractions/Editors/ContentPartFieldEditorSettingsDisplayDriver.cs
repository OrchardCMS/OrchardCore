using System;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentTypes.Editors
{
    /// <summary>
    /// A concrete implementation of <see cref="ContentPartFieldEditorSettingsDisplayDriver"/> provides a driver for all field definitions editors.
    /// </summary>
    public abstract class ContentPartFieldEditorSettingsDisplayDriver : DisplayDriver<ContentPartFieldDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartFieldEditorContext>, IContentPartFieldEditorSettingsDisplayDriver
    {
        protected override void BuildPrefix(ContentPartFieldDefinition model, string htmlFieldPrefix)
        {
            Prefix = $"{model.PartDefinition.Name}.{model.Name}";

            if (!String.IsNullOrEmpty(htmlFieldPrefix))
            {
                Prefix = htmlFieldPrefix + "." + Prefix;
            }
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
    public abstract class ContentPartFieldEditorSettingsDisplayDriver<TField> : ContentPartFieldEditorSettingsDisplayDriver where TField : ContentField
    {
        public override bool CanHandleModel(ContentPartFieldDefinition model)
        {
            return string.Equals(typeof(TField).Name, model.FieldDefinition.Name);
        }
    }
}