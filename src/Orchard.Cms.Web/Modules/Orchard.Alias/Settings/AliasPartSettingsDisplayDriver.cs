using System;
using System.Threading.Tasks;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Alias.Models;

namespace Orchard.Alias.Settings
{
    public class AliasPartSettingsDisplayDriver : ContentTypePartDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(AliasPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape<AliasPartSettingsViewModel>("AliasPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<AliasPartSettings>();

                model.Pattern = settings.Pattern;
                model.AliasPartSettings = settings;

                return Task.CompletedTask;
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