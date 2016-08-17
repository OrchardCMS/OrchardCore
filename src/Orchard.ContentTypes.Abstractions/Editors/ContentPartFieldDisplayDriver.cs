using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentTypes.Editors
{
    /// <summary>
    /// A concrete implementation of <see cref="ContentPartFieldDisplayDriver"/> provides a driver for all field definitions.
    /// </summary>
    public abstract class ContentPartFieldDisplayDriver : DisplayDriver<ContentPartFieldDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartFieldEditorContext>, IContentPartFieldDefinitionDisplayDriver
    {
        public override string GeneratePrefix(ContentPartFieldDefinition model)
        {
            return $"{model.PartDefinition.Name}.{model.Name}";
        }

        public override bool CanHandleModel(ContentPartFieldDefinition model)
        {
            // This drivers applies to all fields.
            return true;
        }
    }

    /// <summary>
    /// A concrete implementation of <see cref="ContentPartFieldDisplayDriver&lt;TField&gt;"/> provides a driver for field definitions
    /// of the type <c>TField</c>.
    /// </summary>
    public abstract class ContentPartFieldDisplayDriver<TField> : ContentPartFieldDisplayDriver where TField : ContentField
    {
        public override bool CanHandleModel(ContentPartFieldDefinition model)
        {
            return string.Equals(typeof(TField).Name, model.FieldDefinition.Name);
        }
    }
}