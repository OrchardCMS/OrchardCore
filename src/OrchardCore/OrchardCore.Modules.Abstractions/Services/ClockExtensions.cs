using System;
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
    }
}