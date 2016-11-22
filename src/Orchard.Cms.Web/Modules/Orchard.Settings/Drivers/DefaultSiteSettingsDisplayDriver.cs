using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Settings.Services;
using Orchard.Settings.ViewModels;

namespace Orchard.Settings.Drivers
{
    public class DefaultSiteSettingsDisplayDriver : SiteSettingsDisplayDriver
    {
        public override IDisplayResult Edit(ISite site, BuildEditorContext context)
        {
            return Combine(
                Shape<SiteSettingsViewModel>("SiteSettings_Edit", model =>
                {
                    model.SiteName = site.SiteName;
                }).Location("Content:1").OnGroup("general"),

                Shape("SiteSettings_SaveButton")
                    .Location("Actions")
                    .OnGroup(context.GroupId) // Trick to render the shape for all groups
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite site, IUpdateModel updater)
        {
            var model = new SiteSettingsViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, t => t.SiteName))
            {
                site.SiteName = model.SiteName;
            }

            return Edit(site);
        }
    }
}
