using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Spatial.Fields;
using OrchardCore.Spatial.Settings;

namespace OrchardCore.Spatial.Drivers
{
    public class GeoPointFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<GeoPointField>
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public GeoPointFieldSettingsDriver(IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<GeoPointFieldSettings>("GeoPointFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.Settings.ToObject<GeoPointFieldSettings>(_jsonSerializerOptions);

                model.Hint = settings.Hint;
                model.Required = settings.Required;
            })
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
