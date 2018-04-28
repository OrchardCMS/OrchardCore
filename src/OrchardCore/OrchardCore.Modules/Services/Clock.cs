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
        public DateTime UtcNow
        {
            get { return GetCurrentInstant().ToDateTimeUtc(); }
        }

        /// <summary>
        /// Returns all relevant TimeZones that are usable "Now"
        /// </summary>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        public ITimeZone[] GetTimeZones(string countryCode)
        {
            var list =
                from location in TzdbDateTimeZoneSource.Default.ZoneLocations
                where string.IsNullOrEmpty(countryCode) ||
                      location.CountryCode.Equals(countryCode,
                                                  StringComparison.OrdinalIgnoreCase)
                let zoneId = location.ZoneId
                let comment = location.Comment
                let tz = DateTimeZoneProviders.Tzdb[zoneId]
                let offset = tz.GetZoneInterval(GetCurrentInstant()).StandardOffset
                orderby offset, zoneId
                select new TimeZone(zoneId, string.Format("({0:+HH:mm}) {1}", offset, zoneId), comment);

            return list.ToArray();
        }

        /// <summary>
        /// Returns a ITimeZone from a timeZone ID string.
        /// If the timeZone string is null or empty then we return the default system ITimeZone
        /// </summary>
        /// <returns></returns>
        public ITimeZone GetLocalTimeZone(string timeZone)
        {
            DateTimeZone dateTimeZone = GetDateTimeZone(timeZone);
            ITimeZone result = GetTimeZones(string.Empty).Where(x => x.Id == dateTimeZone.Id).FirstOrDefault();

            return result;
        }

        public DateTimeOffset ConvertToTimeZone(DateTime? dateTimeUtc, ITimeZone timeZone)
        {
            DateTimeZone dateTimeZone = GetDateTimeZone(timeZone.Id);
            Instant instant = Instant.FromDateTimeUtc(dateTimeUtc ?? UtcNow);
            return instant.InZone(dateTimeZone).ToDateTimeOffset();
        }

        public DateTimeOffset ConvertToTimeZone(DateTimeOffset? dateTimeOffSet, ITimeZone timeZone)
        {
            DateTimeZone dateTimeZone = GetDateTimeZone(timeZone.Id);
            OffsetDateTime offsetDateTime = OffsetDateTime.FromDateTimeOffset(dateTimeOffSet ?? GetCurrentInstant().ToDateTimeOffset());
            return offsetDateTime.InZone(dateTimeZone).ToDateTimeOffset();
        }

        private Instant GetCurrentInstant()
        {
            return SystemClock.Instance.GetCurrentInstant();
        }

        private DateTimeZone GetDateTimeZone(string timeZone)
        {
            //TODO : For backward compatibility find also timezones that are not in Nodatime.
            // see https://github.com/mj1856/TimeZoneConverter
            if (!string.IsNullOrEmpty(timeZone))
            {
                return DateTimeZoneProviders.Tzdb[timeZone];
            }
            else
            {
                return DateTimeZoneProviders.Tzdb.GetSystemDefault();
            }
        }
    }
}