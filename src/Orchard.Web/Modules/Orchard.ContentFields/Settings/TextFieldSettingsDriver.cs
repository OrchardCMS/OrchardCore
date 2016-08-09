using System;
using System.Threading.Tasks;
using Orchard.ContentFields.Fields;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentFields.Settings
{

    public class TextFieldSettingsDisplayDriver : ContentPartFieldDisplayDriver
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            if (!String.Equals(nameof(TextField), partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape<TextFieldSettings>("TextFieldSettings_Edit", model => partFieldDefinition.Settings.Populate(model))
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (!String.Equals(nameof(TextField), partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new TextFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.Hint(model.Hint);

            return Edit(partFieldDefinition);
        }
    }
}
