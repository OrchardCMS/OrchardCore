using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Seo.Models;
using OrchardCore.Seo.ViewModels;

namespace OrchardCore.SeoMeta.Settings
{
    public class SeoMetaPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<SeoMetaPart>
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            return Initialize<SeoMetaPartSettingsViewModel>("SeoMetaPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<SeoMetaPartSettings>();

                model.DisplayKeywords = settings.DisplayKeywords;
                model.DisplayCustomMetaTags = settings.DisplayCustomMetaTags;
                model.DisplayOpenGraph = settings.DisplayOpenGraph;
                model.DisplayTwitter = settings.DisplayTwitter;
                model.DisplayGoogleSchema = settings.DisplayGoogleSchema;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            var model = new SeoMetaPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.DisplayKeywords,
                m => m.DisplayCustomMetaTags,
                m => m.DisplayOpenGraph,
                m => m.DisplayTwitter,
                m => m.DisplayGoogleSchema);

            context.Builder.WithSettings(new SeoMetaPartSettings
            {
                DisplayKeywords = model.DisplayKeywords,
                DisplayCustomMetaTags = model.DisplayCustomMetaTags,
                DisplayOpenGraph = model.DisplayOpenGraph,
                DisplayTwitter = model.DisplayTwitter,
                DisplayGoogleSchema = model.DisplayGoogleSchema
            });

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
