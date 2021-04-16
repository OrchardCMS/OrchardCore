using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Seo.Models;
using OrchardCore.Seo.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace OrchardCore.SeoMeta.Settings
{
    public class SeoMetaPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly ILiquidTemplateManager _templateManager;
        private readonly IStringLocalizer S;

        public SeoMetaPartSettingsDisplayDriver(ILiquidTemplateManager templateManager, IStringLocalizer<SeoMetaPartSettingsDisplayDriver> localizer)
        {
            _templateManager = templateManager;
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(SeoMetaPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

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
            if (!String.Equals(nameof(SeoMetaPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

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
