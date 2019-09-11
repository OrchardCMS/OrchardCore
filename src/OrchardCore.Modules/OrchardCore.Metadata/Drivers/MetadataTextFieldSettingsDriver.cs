using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Metadata.Fields;
using OrchardCore.Metadata.Settings;

namespace OrchardCore.Metadata.Drivers
{
    public class MetadataTextFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MetadataTextField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<MetadataTextFieldSettings>("MetadataTextFieldSettings_Edit", model => partFieldDefinition.Settings.Populate(model))
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new MetadataTextFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.MergeSettings(model);

            return Edit(partFieldDefinition);
        }
    }
}
