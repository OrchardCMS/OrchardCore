using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Markdown.Models;

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


            return Initialize<MarkdownBodyPartSettings>("MarkdownBodyPartSettings_Edit", model => contentTypePartDefinition.PopulateSettings<MarkdownBodyPartSettings>(model))
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(MarkdownBodyPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            var model = new MarkdownBodyPartSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
