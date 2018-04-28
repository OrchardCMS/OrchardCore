using System;
using System.Linq;
using NodaTime;
using NodaTime.TimeZones;

namespace OrchardCore.Modules
{
    public class Clock : IClock
    {
        /// <summary>
        /// Returns a Datetime Kind.Utc that is "Now"
        /// </summary>
        /// <inheritdoc />
        public DateTime UtcNow => GetCurrentInstant().ToDateTimeUtc();

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
            var list =
                from location in TzdbDateTimeZoneSource.Default.ZoneLocations
                where String.IsNullOrEmpty(countryCode) ||
                      location.CountryCode.Equals(countryCode,
                                                  StringComparison.OrdinalIgnoreCase)
                let zoneId = location.ZoneId
                let comment = location.Comment
                let tz = DateTimeZoneProviders.Tzdb[zoneId]
                let offset = tz.GetZoneInterval(GetCurrentInstant()).StandardOffset
                orderby offset, zoneId
                select new TimeZone(zoneId, $"({offset:+HH:mm}) {zoneId}", comment);

            return list.ToArray();
        }

        /// <summary>
        /// Returns a ITimeZone from a timeZone ID string.
        /// If the timeZone string is null or empty then we return the default system ITimeZone
        /// </summary>
        /// <returns></returns>
        public ITimeZone GetLocalTimeZone(string timeZone)
        {
            var dateTimeZone = GetDateTimeZone(timeZone);
            var result = GetTimeZones(String.Empty).FirstOrDefault(x => x.Id == dateTimeZone.Id);

            return result;
        }

        public DateTimeOffset ConvertToTimeZone(DateTime dateTime, ITimeZone timeZone)
        {
            DateTime dateTimeUtc;
            switch (dateTime.Kind)
            {
                case DateTimeKind.Utc:
                    dateTimeUtc = dateTime;
                    break;
                case DateTimeKind.Local:
                    dateTimeUtc = dateTime.ToUniversalTime();
                    break;
                default: //DateTimeKind.Unspecified
                    dateTimeUtc = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                    break;
            }

            var dateTimeZone = GetDateTimeZone(timeZone.Id);
            var instant = Instant.FromDateTimeUtc(dateTimeUtc);
            return instant.InZone(dateTimeZone).ToDateTimeOffset();
        }

        public DateTimeOffset ConvertToTimeZone(DateTimeOffset? dateTimeOffSet, ITimeZone timeZone)
        {
            var dateTimeZone = GetDateTimeZone(timeZone.Id);
            var offsetDateTime = OffsetDateTime.FromDateTimeOffset(dateTimeOffSet ?? GetCurrentInstant().ToDateTimeOffset());
            return offsetDateTime.InZone(dateTimeZone).ToDateTimeOffset();
        }

        private static Instant GetCurrentInstant()
        {
            return SystemClock.Instance.GetCurrentInstant();
        }

        private static DateTimeZone GetDateTimeZone(string timeZone)
        {
            //TODO : For backward compatibility find also timezones that are not in Nodatime.
            // see https://github.com/mj1856/TimeZoneConverter

            return IsValidTimeZone(DateTimeZoneProviders.Tzdb, timeZone) ? DateTimeZoneProviders.Tzdb[timeZone] : DateTimeZoneProviders.Tzdb.GetSystemDefault();
        }

        private static bool IsValidTimeZone(IDateTimeZoneProvider provider, string timeZoneId)
        {
            return provider.GetZoneOrNull(timeZoneId) != null;
        }
    }
}