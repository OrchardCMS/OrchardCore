using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors
{
    public class ContentTypeIndexingSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
        {
            return Initialize<ContentTypeIndexingSettingsViewModel>("ContentTypeIndexingSettings_Edit", model =>
            {
                var settings = contentTypeDefinition.GetSettings<ContentTypeIndexingSettings>();

                model.IsFullTextLiquid = settings.IsFullText;
                model.FullTextLiquid = settings.FullText;
            }).Location("Content:6");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
        {
            var model = new ContentTypeIndexingSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                context.Builder.IsFullText(model.IsFullTextLiquid);
                context.Builder.FullText(model.FullTextLiquid);
            }

            return Edit(contentTypeDefinition, context.Updater);
        }
    }
}
