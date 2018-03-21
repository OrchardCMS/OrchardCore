using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings.ViewModels;
using NodaTime.TimeZones;
using NodaTime;
using System.Linq;
using System.Collections.Generic;

namespace OrchardCore.Settings.Drivers
{
    public class TimeZone
    {
        public string Id;
        public string DisplayName;

        public TimeZone(string id, string displayName)
        {
            this.Id = id;
            this.DisplayName = displayName;
        }
    }

    public class DefaultSiteSettingsDisplayDriver : DisplayDriver<ISite>
    {
        public const string GroupId = "general";

        /// <summary>
        /// Returns a list of valid timezones as a dictionary, where the key is
        /// the timezone id, and the value can be used for display.
        /// </summary>
        /// <param name="countryCode">
        /// The two-letter country code to get timezones for.
        /// Returns all timezones if null or empty.
        /// </param>
        public IEnumerable<TimeZone> GetTimeZones(string countryCode)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var tzdb = DateTimeZoneProviders.Tzdb;

            var list =
                from location in TzdbDateTimeZoneSource.Default.ZoneLocations
                where string.IsNullOrEmpty(countryCode) ||
                      location.CountryCode.Equals(countryCode,
                                                  StringComparison.OrdinalIgnoreCase)
                let zoneId = location.ZoneId
                let tz = tzdb[zoneId]
                let offset = tz.GetZoneInterval(now).StandardOffset
                orderby offset, zoneId
                select new TimeZone(zoneId, string.Format("({0:+HH:mm}) {1}", offset, zoneId));

            return list;
        }

        public override Task<IDisplayResult> EditAsync(ISite site, BuildEditorContext context)
        {

            return Task.FromResult<IDisplayResult>(
                    Shape<SiteSettingsViewModel>("Settings_Edit", model =>
                    {
                        model.SiteName = site.SiteName;
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
