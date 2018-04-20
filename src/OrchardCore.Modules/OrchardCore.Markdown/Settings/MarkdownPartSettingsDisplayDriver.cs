using System;
using System.Threading.Tasks;
using OrchardCore.Markdown.Model;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Markdown.Settings
{
    public class MarkdownPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(MarkdownPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<MarkdownPartSettingsViewModel>("MarkdownPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<MarkdownPartSettings>();

                model.Editor = settings.Editor;
                model.MarkdownPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(MarkdownPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new MarkdownPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Editor))
            {
                context.Builder.WithSettings(new MarkdownPartSettings { Editor = model.Editor });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}