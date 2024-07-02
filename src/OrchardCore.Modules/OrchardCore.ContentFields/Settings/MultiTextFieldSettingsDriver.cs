using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class MultiTextFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MultiTextField>
    {
        protected readonly IStringLocalizer S;

        public MultiTextFieldSettingsDriver(IStringLocalizer<MultiTextFieldSettingsDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<MultiTextFieldSettingsViewModel>("MultiTextFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<MultiTextFieldSettings>();

                model.Required = settings.Required;
                model.Hint = settings.Hint;
            })
            .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new MultiTextFieldSettingsViewModel();
            var settings = new MultiTextFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            settings.Required = model.Required;
            settings.Hint = model.Hint;

            context.Builder.WithSettings(settings);

            return Edit(partFieldDefinition);
        }
    }
}
