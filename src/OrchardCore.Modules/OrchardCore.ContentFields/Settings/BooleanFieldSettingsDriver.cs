using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class BooleanFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<BooleanField>
    {
        public override Task<IDisplayResult> DisplayAsync(ContentPartFieldDefinition partFieldDefinition, BuildDisplayContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<BooleanFieldSettings>("BooleanFieldSettings_Edit", model =>
                {
                    var settings = partFieldDefinition.Settings.ToObject<BooleanFieldSettings>();

                    model.Hint = settings.Hint;
                    model.Label = settings.Label;
                    model.DefaultValue = settings.DefaultValue;
                }).Location("Content")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new BooleanFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return await EditAsync(partFieldDefinition, context);
        }
    }
}
