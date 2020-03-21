using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Metadata.Models;
using OrchardCore.Metadata.ViewModels;

namespace OrchardCore.Metadata.Settings
{
    public class MetadataPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(MetadataPart), contentTypePartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<MetadataPartSettingsViewModel>("MetadataPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.Settings.ToObject<SocialMetadataPartSettings>();

                model.SupportMetaKeywords = settings.SupportMetaKeywords;
                model.SupportOpenGraph = settings.SupportOpenGraph;
                model.SupportTwitterCards = settings.SupportTwitterCards;
                model.MetadataPartSettings = settings;

            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(MetadataPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new MetadataPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.SupportOpenGraph,
                m => m.SupportTwitterCards,
                m => m.SupportMetaKeywords);

            context.Builder.WithSetting(nameof(SocialMetadataPartSettings.SupportOpenGraph), model.SupportOpenGraph.ToString());
            context.Builder.WithSetting(nameof(SocialMetadataPartSettings.SupportTwitterCards), model.SupportTwitterCards.ToString());
            context.Builder.WithSetting(nameof(SocialMetadataPartSettings.SupportMetaKeywords), model.SupportMetaKeywords.ToString());


            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
