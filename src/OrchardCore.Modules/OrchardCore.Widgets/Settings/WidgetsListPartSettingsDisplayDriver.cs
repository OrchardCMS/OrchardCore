using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Widgets.Models;

namespace OrchardCore.Widgets.Settings
{
    public class WidgetsListPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<WidgetsListPart>
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            return Initialize<WidgetsListPartSettingsViewModel>("WidgetsPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<WidgetsListPartSettings>();

                model.Zones = String.Join(", ", settings.Zones);
                model.WidgetsListPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            var model = new WidgetsListPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Zones))
            {
                context.Builder.WithSettings(new WidgetsListPartSettings { Zones = (model.Zones ?? "").Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries) });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
