using System;
using System.Threading.Tasks;

namespace OrchardCore.Modules
{
    public static class ClockExtensions
    {
        /// <summary>
        /// Converts a <see cref="DateTime" /> to the specified <see cref="ITimeZone" /> instance.
        /// </summary>
        public static DateTimeOffset ConvertToTimeZone(this IClock clock, DateTime dateTime, ITimeZone timeZone)
        {
            var dateTimeUtc = dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),

                // 'DateTimeKind.Unspecified'.
                _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            };

            return clock.ConvertToTimeZone(new DateTimeOffset(dateTimeUtc), timeZone);
        }

        /// <summary>
        /// Converts a <see cref="DateTime" /> to the specified <see cref="ITimeZone" /> instance.
        /// </summary>
        public static Task<DateTimeOffset> ConvertToLocalAsync(this ILocalClock localClock, DateTime dateTime)
        {
            var dateTimeUtc = dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),

                // 'DateTimeKind.Unspecified'.
                _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            };

            return localClock.ConvertToLocalAsync(new DateTimeOffset(dateTimeUtc));
        }
    }
}
