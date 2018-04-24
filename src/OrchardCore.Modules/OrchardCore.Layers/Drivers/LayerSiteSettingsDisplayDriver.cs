using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Layers.Drivers
{
    public class LayerSiteSettingsDisplayDriver : SectionDisplayDriver<ISite, LayerSettings>
    {
        public const string GroupId = "layers";

        public override IDisplayResult Edit(LayerSettings settings, BuildEditorContext context)
        {
            return Initialize<LayerSettingsViewModel>("LayerSettings_Edit", model =>
                {
                    model.Zones = String.Join(", ", settings.Zones);
                }).Location("Content:3").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(LayerSettings settings, IUpdateModel updater, string groupId)
        {
            if (groupId == GroupId)
            {
                var model = new LayerSettingsViewModel();

                await updater.TryUpdateModelAsync(model, Prefix);

                settings.Zones = model.Zones.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return Edit(settings);
        }
    }
}
