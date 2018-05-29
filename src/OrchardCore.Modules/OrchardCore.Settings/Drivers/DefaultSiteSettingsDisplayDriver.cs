using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings.ViewModels;
using OrchardCore.Modules;

namespace OrchardCore.Settings.Drivers
{
    public class DefaultSiteSettingsDisplayDriver : DisplayDriver<ISite>
    {
        private readonly IClock _clock;
        public const string GroupId = "general";

        public DefaultSiteSettingsDisplayDriver(IClock clock)
        {
            _clock = clock;
        }

        public override Task<IDisplayResult> EditAsync(ISite site, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                    Initialize<SiteSettingsViewModel>("Settings_Edit", model =>
                    {
                        model.SiteName = site.SiteName;
                        model.BaseUrl = site.BaseUrl;
                        model.TimeZone = site.TimeZoneId;
                        model.TimeZones = _clock.GetTimeZones();
                        model.SiteCulture = site.Culture;
                    }).Location("Content:1").OnGroup(GroupId)
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite site, UpdateEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                var model = new SiteSettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix, t => t.SiteName, t => t.BaseUrl, t => t.TimeZone, t => t.SiteCulture))
                {
                    site.SiteName = model.SiteName;
                    site.BaseUrl = model.BaseUrl;
                    site.TimeZoneId = model.TimeZone;
                    site.Culture = model.SiteCulture;
                }
            }

            return Edit(site);
        }
    }
}
