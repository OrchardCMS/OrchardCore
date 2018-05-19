using System;
using System.Threading.Tasks;
using OrchardCore.Modules;

namespace OrchardCore.Modules
{
    public static class ClockExtensions
    {
        /// <summary>
        /// Converts a <see cref="DateTime" /> to the specified <see cref="ITimeZone" /> instance.
        /// </summary>
        public static DateTimeOffset ConvertToTimeZone(this IClock clock, DateTime dateTime, ITimeZone timeZone)
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

            return clock.ConvertToTimeZone(new DateTimeOffset(dateTime), timeZone);
        }

        /// <summary>
        /// Converts a <see cref="DateTime" /> to the specified <see cref="ITimeZone" /> instance.
        /// </summary>
        public static Task<DateTimeOffset> ConvertToLocalAsync(this ILocalClock localClock, DateTime dateTime)
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

            return localClock.ConvertToLocalAsync(new DateTimeOffset(dateTime));
        }
    }
}