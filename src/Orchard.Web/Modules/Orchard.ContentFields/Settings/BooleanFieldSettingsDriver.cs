using System;
using System.Threading.Tasks;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentFields.Settings
{

    public class BooleanFieldSettingsDisplayDriver : ContentPartFieldDisplayDriver
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            if (!String.Equals("BooleanField", partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape<BooleanFieldSettings>("BooleanFieldSettings_Edit", model => partFieldDefinition.Settings.Populate(model))
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (!String.Equals("BooleanField", partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new BooleanFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.Hint(model.Hint);

            return Edit(partFieldDefinition);
        }
    }
}
