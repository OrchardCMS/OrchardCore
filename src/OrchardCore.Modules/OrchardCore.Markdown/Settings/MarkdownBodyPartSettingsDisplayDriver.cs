using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Settings
{
    public class MarkdownBodyPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(MarkdownBodyPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            return Initialize<MarkdownBodyPartSettingsViewModel>("MarkdownBodyPartSettings_Edit", model =>
                {
                    var settings = contentTypePartDefinition.GetSettings<MarkdownBodyPartSettings>();

                    model.SanitizeHtml = settings.SanitizeHtml;
                })
                .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(MarkdownBodyPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            var model = new MarkdownBodyPartSettingsViewModel();
            var settings = new MarkdownBodyPartSettings();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                settings.SanitizeHtml = model.SanitizeHtml;

                context.Builder.WithSettings(settings);
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
