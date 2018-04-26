using System;
using System.Collections.Generic;
using System.Globalization;
using NodaTime;
using NodaTime.TimeZones;

namespace OrchardCore.Modules
{
    public class Clock : IClock
    {
        public DateTimeZone TimeZone { get; protected set; }
        public CultureInfo Culture { get; protected set; }

        public Clock()
        {
            TimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            Culture = CultureInfo.InvariantCulture;
        }

        public IDateTimeZoneProvider Tzdb
        {
            get
            {
                return DateTimeZoneProviders.Tzdb;
            }
        }

        public IEnumerable<TzdbZoneLocation> TimeZones
        {
            get
            {
                return TzdbDateTimeZoneSource.Default.ZoneLocations;
            }
        }

        public DateTime UtcNow
        {
            get { return GetCurrentInstant().ToDateTimeUtc(); }
        }

        public Instant InstantNow
        {
            get { return GetCurrentInstant(); }
        }

        public Instant GetCurrentInstant()
        {
            return SystemClock.Instance.GetCurrentInstant();
        }

        public DateTimeOffset ToDateTimeOffset(OffsetDateTime offsetDateTime)
        {
            return offsetDateTime.ToDateTimeOffset();
        }

        public LocalDateTime LocalNow
        {
            get
            {
                return GetCurrentInstant().InZone(TimeZone).LocalDateTime;
            }
        }

        public Instant ToInstant(LocalDateTime local)
        {
            return local.InZone(TimeZone, Resolvers.LenientResolver).ToInstant();
        }

        public LocalDateTime ToLocal(Instant instant)
        {
            return instant.InZone(TimeZone).LocalDateTime;
        }

        public ZonedDateTime ToZonedDateTime(DateTime? dateTime)
        {
            Instant instant = Instant.FromDateTimeUtc(dateTime ?? UtcNow);
            return instant.InZone(TimeZone);
        }

        public ZonedDateTime ToZonedDateTime(DateTimeOffset? dateTimeOffSet)
        {
            OffsetDateTime offsetDateTime = OffsetDateTime.FromDateTimeOffset(dateTimeOffSet ?? InstantNow.ToDateTimeOffset());
            return offsetDateTime.InZone(TimeZone);
        }

        public void SetDateTimeZone(string timeZone)
        {
            TimeZone = GetDateTimeZone(timeZone);
        }

        public DateTimeZone GetDateTimeZone(string timeZone)
        {
            //TODO : For backward compatibility find also timezones that are not in Nodatime.
            // see https://github.com/mj1856/TimeZoneConverter
            return DateTimeZoneProviders.Tzdb[timeZone];
        }

        public void SetCulture(CultureInfo culture)
        {
            Culture = culture;
        }

    }
}