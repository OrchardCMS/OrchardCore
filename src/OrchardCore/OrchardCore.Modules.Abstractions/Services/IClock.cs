using System;
using NodaTime;

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
        DateTimeZone TimeZone { get; set; }
        DateTimeZone GetDateTimeZone(string timeZone);
        LocalDateTime LocalNow { get; }
        Instant ToInstant(LocalDateTime local);
        LocalDateTime ToLocal(Instant instant);
        ZonedDateTime ToZonedDateTime(DateTime? dateTime);
        
    }
}