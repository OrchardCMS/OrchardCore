using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class DateTimeFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<DateTimeField>
    {
        public override Task<IDisplayResult> EditAsync(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<DateTimeFieldSettings>("DateTimeFieldSettings_Edit", model =>
                {
                    var settings = partFieldDefinition.Settings.ToObject<DateTimeFieldSettings>();

                    model.Hint = settings.Hint;
                    model.Required = settings.Required;
                }).Location("Content")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new DateTimeFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return await EditAsync(partFieldDefinition, context);
        }
    }
}
