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

        public DefaultSiteSettingsDisplayDriver(IClock clock) {
            _clock = clock;
        }

        /// <summary>
        /// Returns a list of valid timezones as a ITimeZone[], where the key is
        /// the timezone id(string), and the value can be used for display. The list is filtered to contain only
        /// choices that are reasonably valid for the present and near future for real places. The list is
        /// also sorted first by UTC Offset and then by timezone name.
        /// </summary>
        /// <param name="countryCode">
        /// The two-letter country code to get timezones for.
        /// Returns all timezones if null or empty.
        /// </param>
        public ITimeZone[] GetTimeZones(string countryCode)
        {
            var list = _clock.GetTimeZones(countryCode);

            return list;
        }

        public override Task<IDisplayResult> EditAsync(ISite site, BuildEditorContext context)
        {

            return Task.FromResult<IDisplayResult>(
                    Initialize<SiteSettingsViewModel>("Settings_Edit", model =>
                    {
                        model.SiteName = site.SiteName;
                        model.BaseUrl = site.BaseUrl;
                        model.TimeZone = site.TimeZone;
                        model.TimeZones = GetTimeZones("");
                    }).Location("Content:1").OnGroup(GroupId)
            );
        }
        public override async Task<IDisplayResult> UpdateAsync(ISite site, UpdateEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                var model = new SiteSettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix, t => t.SiteName, t => t.BaseUrl, t => t.TimeZone))
                {
                    site.SiteName = model.SiteName;
                    site.BaseUrl = model.BaseUrl;
                    site.TimeZone = model.TimeZone;
                }
            }

            return Edit(site);
        }
    }
}
