using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings.ViewModels;
using System.Linq;
using System.Collections.Generic;
using OrchardCore.Modules;

namespace OrchardCore.Settings.Drivers
{
    public class DefaultSiteSettingsDisplayDriver : DisplayDriver<ISite>
    {
        public const string GroupId = "general";
        public IClock _clock;

        public DefaultSiteSettingsDisplayDriver(IClock clock) {
            _clock = clock;
        }

        /// <summary>
        /// Returns a list of valid timezones as a IEnumerable<TimeZoneViewModel>, where the key is
        /// the timezone id(string), and the value can be used for display. The list is filtered to contain only
        /// choices that are reasonably valid for the present and near future for real places. The list is
        /// also sorted first by UTC Offset and then by timezone name.
        /// </summary>
        /// <param name="countryCode">
        /// The two-letter country code to get timezones for.
        /// Returns all timezones if null or empty.
        /// </param>
        public IEnumerable<TimeZoneViewModel> GetTimeZones(string countryCode)
        {
            var now = _clock.InstantNow;
            var tzdb = _clock.Tzdb;

            var list =
                from location in _clock.TimeZones
                where string.IsNullOrEmpty(countryCode) ||
                      location.CountryCode.Equals(countryCode,
                                                  StringComparison.OrdinalIgnoreCase)
                let zoneId = location.ZoneId
                let comment = location.Comment
                let tz = tzdb[zoneId]
                let offset = tz.GetZoneInterval(now).StandardOffset
                orderby offset, zoneId
                select new TimeZoneViewModel(zoneId, string.Format("({0:+HH:mm}) {1}", offset, zoneId), comment);

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
