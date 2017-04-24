using System;
using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Layers.Models;
using Orchard.Layers.ViewModels;
using Orchard.Settings.Services;

namespace Orchard.Layers.Drivers
{
    public class LayerSiteSettingsDisplayDriver : SiteSettingsSectionDisplayDriver<LayerSettings>
    {
        public const string GroupId = "layers";

        public override IDisplayResult Edit(LayerSettings settings, BuildEditorContext context)
        {
            return Shape<LayerSettingsViewModel>("LayerSettings_Edit", model =>
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
