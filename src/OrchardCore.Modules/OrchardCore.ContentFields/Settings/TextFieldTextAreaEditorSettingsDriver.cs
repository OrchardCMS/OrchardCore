using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{

    public class TextFieldTextAreaEditorSettingsDriver : ContentPartFieldEditorSettingsDisplayDriver<TextField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<TextFieldTextAreaEditorSettings>("TextFieldTextAreaEditorSettings_Edit", model => partFieldDefinition.Settings.Populate<TextFieldTextAreaEditorSettings>(model))
                .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "TextArea")
            {
                var model = new TextFieldTextAreaEditorSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                context.Builder.WithSettings(model);
            }

            return Edit(partFieldDefinition);
        }
    }
}
