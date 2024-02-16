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
    public class NumericFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<NumericField>
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public NumericFieldSettingsDriver(IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<NumericFieldSettings>("NumericFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.Settings.ToObject<NumericFieldSettings>(_jsonSerializerOptions);

                model.Hint = settings.Hint;
                model.Required = settings.Required;
                model.Scale = settings.Scale;
                model.Minimum = settings.Minimum;
                model.Maximum = settings.Maximum;
                model.Placeholder = settings.Placeholder;
                model.DefaultValue = settings.DefaultValue;
            })
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new NumericFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return Edit(partFieldDefinition);
        }
    }
}
