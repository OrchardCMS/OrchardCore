using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class FontIconPickerFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<FontIconPickerField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<FontIconPickerFieldSettings>("FontIconPickerFieldSettings_Edit", model =>
             {
                 partFieldDefinition.Settings.Populate(model);
             }).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new FontIconPickerFieldSettings();
            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.MergeSettings(model);

            return Edit(partFieldDefinition);
        }
    }
}
