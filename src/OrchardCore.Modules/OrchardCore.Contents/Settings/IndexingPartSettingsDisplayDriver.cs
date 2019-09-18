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
    public class IndexingPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public IndexingPartSettingsDisplayDriver(IStringLocalizer<IndexingPartSettingsDisplayDriver> localizer)
        {
            S = localizer;
        }

        public IStringLocalizer S { get; set; }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(IndexingPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<IndexingPartSettingsViewModel>("IndexingPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<IndexingPartSettings>();
                model.IsNotIndexingFullTextOrAll = settings.IsNotIndexingFullTextOrAll;
                model.IndexDisplayText = settings.IndexDisplayText;
                model.IndexBodyAspect = settings.IndexBodyAspect;

            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(IndexingPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new IndexingPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                context.Builder.WithSettings(new IndexingPartSettings { IsNotIndexingFullTextOrAll = model.IsNotIndexingFullTextOrAll, IndexDisplayText = model.IndexDisplayText, IndexBodyAspect = model.IndexBodyAspect });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}