using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class HtmlFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<HtmlField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<HtmlSettingsViewModel>("HtmlFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<HtmlFieldSettings>();

                model.AllowCustomScripts = settings.AllowCustomScripts;
                model.Hint = settings.Hint;
            })
            .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new HtmlSettingsViewModel();
            var settings = new HtmlFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            settings.AllowCustomScripts = model.AllowCustomScripts;
            settings.Hint = model.Hint;

            context.Builder.WithSettings(settings);

            return Edit(partFieldDefinition);
        }
    }
}
