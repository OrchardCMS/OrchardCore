using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Spatial.Fields;
using OrchardCore.Spatial.Settings;
using OrchardCore.Spatial.ViewModels;

namespace OrchardCore.Spatial.Drivers
{
    public class GeoPointFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<GeoPointField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<GeoPointFieldSettings>("GeoPointFieldSettings_Edit", model => partFieldDefinition.PopulateSettings(model))
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new GeoPointFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return Edit(partFieldDefinition);
        }
    }
}
