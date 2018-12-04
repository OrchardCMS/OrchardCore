using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Alias.Models;

namespace OrchardCore.Alias.Settings
{
    public class AliasPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(AliasPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<AliasPartSettingsViewModel>("AliasPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<AliasPartSettings>();

                model.Pattern = settings.Pattern;
                model.AliasPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(AliasPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new AliasPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Pattern))
            {
                context.Builder.WithSettings(new AliasPartSettings { Pattern = model.Pattern });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}