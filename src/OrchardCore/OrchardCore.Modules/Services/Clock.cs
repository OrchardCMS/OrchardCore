using System;
using System.Linq;
using NodaTime;
using NodaTime.TimeZones;

namespace OrchardCore.Modules
{
    public class Clock : IClock
    {
        private static Instant CurrentInstant => SystemClock.Instance.GetCurrentInstant();

        /// <summary>
        /// Returns a Datetime Kind.Utc that is "Now"
        /// </summary>
        /// <inheritdoc />
        public DateTime UtcNow => CurrentInstant.ToDateTimeUtc();

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
        public ITimeZone[] GetTimeZones()
        {
            var list =
                from location in TzdbDateTimeZoneSource.Default.ZoneLocations
                let zoneId = location.ZoneId
                let tz = DateTimeZoneProviders.Tzdb[zoneId]
                let offset = tz.GetZoneInterval(CurrentInstant).StandardOffset
                orderby offset, zoneId
                select new TimeZone(zoneId, offset);

            return list.ToArray();
        }

        public ITimeZone GetTimeZone(string timeZoneId)
        {
            if (String.IsNullOrEmpty(timeZoneId))
            {
                return GetSystemTimeZone();
            }

            var dateTimeZone = GetDateTimeZone(timeZoneId);
            var result = GetTimeZones().FirstOrDefault(x => x.TimeZoneId == dateTimeZone.Id);

            //If TimeZone is not found in default ZoneLocations list then retrieve it from the Tzdb
            if (result == null)
            {
                var offset = DateTimeZoneProviders.Tzdb[dateTimeZone.Id].GetZoneInterval(CurrentInstant).StandardOffset;
                return new TimeZone(dateTimeZone.Id, offset);
            }
            else
            {
                return result;
            }
        }

        public ITimeZone GetSystemTimeZone()
        {
            var timezone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            return GetTimeZone(timezone.Id);
        }

        public DateTimeOffset ConvertToTimeZone(DateTimeOffset dateTimeOffSet, ITimeZone timeZone)
        {
            var dateTimeZone = GetDateTimeZone(timeZone.TimeZoneId);
            var offsetDateTime = OffsetDateTime.FromDateTimeOffset(dateTimeOffSet);
            return offsetDateTime.InZone(dateTimeZone).ToDateTimeOffset();
        }

        private static DateTimeZone GetDateTimeZone(string timeZone)
        {
            if (String.IsNullOrEmpty(timeZone))
            {
                return IsValidTimeZone(DateTimeZoneProviders.Tzdb, timeZone)
                    ? DateTimeZoneProviders.Tzdb[timeZone]
                    : DateTimeZoneProviders.Tzdb.GetSystemDefault();
            }
            else
            {
                return DateTimeZoneProviders.Tzdb.GetSystemDefault();
            }
        }

        private static bool IsValidTimeZone(IDateTimeZoneProvider provider, string timeZoneId)
        {
            return provider.GetZoneOrNull(timeZoneId) != null;
        }
    }
}