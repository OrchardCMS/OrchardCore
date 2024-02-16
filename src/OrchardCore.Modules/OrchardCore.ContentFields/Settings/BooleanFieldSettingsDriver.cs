using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class BooleanFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<BooleanField>
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public BooleanFieldSettingsDriver(IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<BooleanFieldSettings>("BooleanFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.Settings.ToObject<BooleanFieldSettings>(_jsonSerializerOptions);

                model.Hint = settings.Hint;
                model.Label = settings.Label;
                model.DefaultValue = settings.DefaultValue;
            })
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new BooleanFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return Edit(partFieldDefinition);
        }
    }
}
