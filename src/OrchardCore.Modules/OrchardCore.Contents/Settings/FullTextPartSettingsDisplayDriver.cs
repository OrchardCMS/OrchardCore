using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Lists.Settings
{
    public class FullTextPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public FullTextPartSettingsDisplayDriver(IStringLocalizer<FullTextPartSettingsDisplayDriver> localizer)
        {
            TS = localizer;
        }

        public IStringLocalizer TS { get; set; }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(FullTextPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<FullTextPartSettingsViewModel>("FullTextPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<FullTextPartSettings>();
                model.IndexFullOrFullText = settings.IndexFullOrFullText;

            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(FullTextPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new FullTextPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                context.Builder.WithSettings(new FullTextPartSettings { IndexFullOrFullText = model.IndexFullOrFullText});
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}