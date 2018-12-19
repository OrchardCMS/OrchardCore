using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Spatial.Fields;

namespace OrchardCore.Spatial.Settings
{

    public class GeoPointFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<Fields.GeoPointField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<GeoPointFieldSettings>("GeoPointFieldSettings_Edit", model => partFieldDefinition.Settings.Populate(model))
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new GeoPointFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.MergeSettings(model);

            return Edit(partFieldDefinition);
        }
    }
}
