using System;
using System.Threading.Tasks;
using Orchard.Widgets.Models;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Widgets.Settings
{
    public class WidgetsListPartSettingsDisplayDriver : ContentTypePartDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(WidgetsListPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape<WidgetsListPartSettingsViewModel>("WidgetsPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<WidgetsListPartSettings>();

                model.Zones = String.Join(", ", settings.Zones);
                model.WidgetsListPartSettings = settings;

                return Task.CompletedTask;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(WidgetsListPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new WidgetsListPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Zones))
            {
                context.Builder.WithSettings(new WidgetsListPartSettings { Zones = (model.Zones ?? "").Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries) });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}