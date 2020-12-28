using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class MultiTextFieldCheckboxListEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MultiTextField>
    {
        private readonly IStringLocalizer S;

        public MultiTextFieldCheckboxListEditorSettingsDriver(IStringLocalizer<MultiTextFieldCheckboxListEditorSettingsDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<CheckboxListSettingsViewModel>("MultiTextFieldCheckboxListEditorSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<MultiTextFieldCheckboxListEditorSettings>();

                model.Direction = settings.Direction;
            })
            .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "CheckboxList")
            {
                var model = new CheckboxListSettingsViewModel();
                var settings = new MultiTextFieldCheckboxListEditorSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.Direction = model.Direction;

                context.Builder.WithSettings(settings);
            }

            return Edit(partFieldDefinition);
        }
    }
}
