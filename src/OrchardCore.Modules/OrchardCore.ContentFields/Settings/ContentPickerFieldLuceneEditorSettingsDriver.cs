using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{

    public class ContentPickerFieldLuceneEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<ContentPickerField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<ContentPickerFieldLuceneEditorSettings>("ContentPickerFieldLuceneEditorSettings_Edit", model =>
                partFieldDefinition.Settings.Populate<ContentPickerFieldLuceneEditorSettings>(model)).Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "Lucene")
            {
                var model = new ContentPickerFieldLuceneEditorSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                context.Builder.WithSettings(model);
            }

            return Edit(partFieldDefinition);
        }
    }
}
