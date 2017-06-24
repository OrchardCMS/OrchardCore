using System;
using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Views;
using Orchard.Settings.ViewModels;

namespace Orchard.Settings.Drivers
{
    public class DefaultSiteSettingsDisplayDriver : DisplayDriver<ISite>
    {
        public const string GroupId = "general";

        public override Task<IDisplayResult> EditAsync(ISite site, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Combine(
                    Shape<SiteSettingsViewModel>("Settings_Edit", model =>
                    {
                        model.SiteName = site.SiteName;
                        model.TimeZone = site.TimeZone;
                        model.TimeZones = TimeZoneInfo.GetSystemTimeZones();
                    }).Location("Content:1").OnGroup(GroupId),

                    Shape("SiteSettings_SaveButton")
                        .Location("Actions")
                        .OnGroup(context.GroupId) // Trick to render the shape for all groups
                )
            );
        }
        public override async Task<IDisplayResult> UpdateAsync(ISite site, UpdateEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                var model = new SiteSettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix, t => t.SiteName, t => t.TimeZone))
                {
                    site.SiteName = model.SiteName;
                    site.TimeZone = model.TimeZone;
                }
            }

            return Edit(site);
        }
    }
}
