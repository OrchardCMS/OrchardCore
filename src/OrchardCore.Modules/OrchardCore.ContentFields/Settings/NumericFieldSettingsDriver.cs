using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class NumericFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<NumericField>
    {
        public override Task<IDisplayResult> EditAsync(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<NumericFieldSettings>("NumericFieldSettings_Edit", model =>
                {
                    var settings = partFieldDefinition.Settings.ToObject<NumericFieldSettings>();

                    model.Hint = settings.Hint;
                    model.Required = settings.Required;
                    model.Scale = settings.Scale;
                    model.Minimum = settings.Minimum;
                    model.Maximum = settings.Maximum;
                    model.Placeholder = settings.Placeholder;
                    model.DefaultValue = settings.DefaultValue;
                }).Location("Content")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new NumericFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return await EditAsync(partFieldDefinition, context);
        }
    }
}
