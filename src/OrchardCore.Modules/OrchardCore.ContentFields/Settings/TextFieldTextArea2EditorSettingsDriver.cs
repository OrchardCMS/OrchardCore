using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{

    public class TextFieldTextArea2EditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<TextField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<TextFieldTextArea2EditorSettingsViewModel>("TextFieldTextArea2EditorSettings_Edit", model => partFieldDefinition.Settings.Populate<TextFieldTextArea2EditorSettingsViewModel>(model))
                .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "TextArea2")
            {
                var model = new TextFieldTextArea2EditorSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                context.Builder.WithSettings(model.TextFieldTextArea2EditorSettings);
            }

            return Edit(partFieldDefinition);
        }
    }
}
