using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class KeyValuePairsFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<KeyValuePairsField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<KeyValuePairsFieldSettingsViewModel>("KeyValuePairsFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<KeyValuePairsFieldSettings>();

                model.Required = settings.Required;
                model.Hint = settings.Hint;
            })
            .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new KeyValuePairsFieldSettingsViewModel();
            var settings = new KeyValuePairsFieldSettings();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                settings.Required = model.Required;
                settings.Hint = model.Hint;

                context.Builder.WithSettings(settings);
            }

            return Edit(partFieldDefinition);
        }
    }
}
