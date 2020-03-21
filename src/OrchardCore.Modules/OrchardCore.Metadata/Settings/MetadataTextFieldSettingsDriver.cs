using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Metadata.Fields;

namespace OrchardCore.Metadata.Settings
{
    public class MetadataTextFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MetadataTextField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<MetadataTextFieldSettings>("MetadataTextFieldSettings_Edit", model => partFieldDefinition.PopulateSettings(model))
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new MetadataTextFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return Edit(partFieldDefinition);
        }
    }
}
