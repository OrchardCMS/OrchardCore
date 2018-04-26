using System;
using System.Collections.Generic;
using System.Globalization;
using NodaTime;
using NodaTime.TimeZones;

namespace OrchardCore.Modules
{
	/// <summary>
	/// Provides the current Utc <see cref="DateTime"/>, and time related method for cache management.
	/// This service should be used whenever the current date and time are needed, instead of <seealso cref="DateTime"/> directly.
	/// It also makes implementations more testable, as time can be mocked.
	/// </summary>
	public interface IClock : NodaTime.IClock
    {
		/// <summary>
		/// Gets the current <see cref="DateTime"/> of the system, expressed in Utc
		/// </summary>
		DateTime UtcNow { get; }

        Instant InstantNow { get; }

        IDateTimeZoneProvider Tzdb { get; }

        IEnumerable<TzdbZoneLocation> TimeZones { get; }

        /// <summary>
        /// Gets the current <see cref="DateTimeZone"/> of the system
        /// </summary>
        DateTimeZone TimeZone { get; }

        /// <summary>
        /// Gets the current clock culture
        /// </summary>
        CultureInfo Culture { get; }

        /// <summary>
        /// Gets the current <see cref="LocalDateTime"/> of the system
        /// </summary>
        LocalDateTime LocalNow { get; }

        void SetDateTimeZone(string timeZone);
        DateTimeZone GetDateTimeZone(string timeZone);
        void SetCulture(CultureInfo culture);

        Instant ToInstant(LocalDateTime local);
        LocalDateTime ToLocal(Instant instant);
        ZonedDateTime ToZonedDateTime(DateTime? dateTime);
        ZonedDateTime ToZonedDateTime(DateTimeOffset? dateTimeOffSet);
    }
}