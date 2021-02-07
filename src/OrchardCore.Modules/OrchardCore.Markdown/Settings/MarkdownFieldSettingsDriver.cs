using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Settings
{
    public class MarkdownFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MarkdownField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<MarkdownFieldSettingsViewModel>("MarkdownFieldSettings_Edit", model =>
            {

                var settings = partFieldDefinition.GetSettings<MarkdownFieldSettings>();

                model.SanitizeHtml = settings.SanitizeHtml;
                model.Hint = settings.Hint;
            }).Location("Content:20");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new MarkdownFieldSettingsViewModel();
            var settings = new MarkdownFieldSettings();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                settings.SanitizeHtml = model.SanitizeHtml;
                settings.Hint = model.Hint;

                context.Builder.WithSettings(settings);
            }

            return Edit(partFieldDefinition);
        }
    }
}
