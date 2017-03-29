using System;
using System.Threading.Tasks;
using Orchard.Markdown.Model;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Markdown.Settings
{
    public class MarkdownPartSettingsDisplayDriver : ContentTypePartDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(MarkdownPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape<MarkdownPartSettingsViewModel>("MarkdownPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<MarkdownPartSettings>();

                model.RenderTokens = settings.RenderTokens;
                model.Editor = settings.Editor;
                model.MarkdownPartSettings = settings;

                return Task.CompletedTask;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(MarkdownPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new MarkdownPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.RenderTokens, m => m.Editor))
            {
                context.Builder.WithSettings(new MarkdownPartSettings { RenderTokens = model.RenderTokens, Editor = model.Editor });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}