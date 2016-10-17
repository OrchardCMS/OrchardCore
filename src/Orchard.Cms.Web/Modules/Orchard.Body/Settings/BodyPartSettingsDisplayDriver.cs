using System;
using System.Threading.Tasks;
using Orchard.Body.Model;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Body.Settings
{
    public class BodyPartSettingsDisplayDriver : ContentTypePartDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(BodyPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape<BodyPartSettingsViewModel>("BodyPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.Settings.ToObject<BodyPartSettings>();

                model.RenderTokens = settings.RenderTokens;
                model.BodyPartSettings = settings;

                return Task.CompletedTask;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(BodyPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new BodyPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.RenderTokens);

            context.Builder.WithSetting(nameof(BodyPartSettings.RenderTokens), model.RenderTokens.ToString());

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}