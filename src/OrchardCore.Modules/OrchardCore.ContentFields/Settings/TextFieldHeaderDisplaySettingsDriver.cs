using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class TextFieldHeaderDisplaySettingsDriver : ContentPartFieldDefinitionDisplayDriver<TextField>
    {
        public override Task<IDisplayResult> EditAsync(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<HeaderSettingsViewModel>("TextFieldHeaderDisplaySettings_Edit", model =>
                {
                    var settings = partFieldDefinition.GetSettings<TextFieldHeaderDisplaySettings>();

                    model.Level = settings.Level;
                }).Location("DisplayMode")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.DisplayMode() == "Header")
            {
                var model = new HeaderSettingsViewModel();
                var settings = new TextFieldHeaderDisplaySettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.Level = model.Level;

                context.Builder.WithSettings(settings);
            }

            return await EditAsync(partFieldDefinition, context);
        }
    }
}
