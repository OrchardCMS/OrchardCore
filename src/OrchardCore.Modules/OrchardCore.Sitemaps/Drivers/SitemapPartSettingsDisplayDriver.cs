using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Drivers
{
    public class SitemapPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(SitemapPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<SitemapPartSettingsViewModel>("SitemapPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.Settings.ToObject<SitemapPartSettings>();

                model.ExcludePart = settings.ExcludePart;
                model.ExcludeByDefault = settings.ExcludeByDefault;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(SitemapPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new SitemapPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, 
                m => m.ExcludePart,
                m => m.ExcludeByDefault);

            context.Builder.WithSetting(nameof(SitemapPartSettings.ExcludePart), model.ExcludePart.ToString());
            context.Builder.WithSetting(nameof(SitemapPartSettings.ExcludeByDefault), model.ExcludeByDefault.ToString());

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}