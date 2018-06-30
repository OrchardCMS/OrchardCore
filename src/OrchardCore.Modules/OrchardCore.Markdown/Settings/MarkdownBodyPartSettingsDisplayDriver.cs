using System;
using System.Threading.Tasks;
using OrchardCore.Markdown.Model;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Markdown.Settings
{
    public class MarkdownBodyPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(MarkdownBodyPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<MarkdownBodyPartSettingsViewModel>("MarkdownBodyPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<MarkdownBodyPartSettings>();

                model.Editor = settings.Editor;
                model.MarkdownBodyPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(MarkdownBodyPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new MarkdownBodyPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Editor))
            {
                context.Builder.WithSettings(new MarkdownBodyPartSettings { Editor = model.Editor });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}