using System;
using NodaTime;
using NodaTime.TimeZones;

namespace OrchardCore.Modules
{
    public class Clock : IClock
    {
        public DateTimeZone TimeZone { get; set; }

        public Instant GetCurrentInstant()
        {
            return GetCurrentInstant();
        }

        public DateTime UtcNow
        {
            get { return GetCurrentInstant().ToDateTimeUtc(); }
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

        public DateTimeZone GetDateTimeZone(string timeZone) {
            return DateTimeZoneProviders.Tzdb[timeZone];
        }

    }
}