using System;

namespace OrchardCore.Modules
{
    /// <summary>
    /// Provides the current Utc <see cref="DateTime"/>, and time related method for cache management.
    /// This service should be used whenever the current date and time are needed, instead of <seealso cref="DateTime"/> directly.
    /// It also makes implementations more testable, as time can be mocked.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the current <see cref="DateTime"/> of the system, expressed in Utc
        /// We don't return a DateTimeOffset here since DateTimeOffset.DateTime and DateTimeOffset.LocalDateTime
        /// would return the same DateTime with a Offset of 0.
        /// </summary>
        DateTime UtcNow { get; }

        /// <summary>
        /// Returns the list of all available <see cref="ITimeZone" />.
        /// </summary>
        ITimeZone[] GetTimeZones();

        /// <summary>
        /// Returns a <see cref="ITimeZone" /> from a time zone id or the local system's one if not found.
        /// </summary>
        ITimeZone GetTimeZone(string timeZoneId);

        /// <summary>
        /// Returns a default <see cref="ITimeZone" /> for the system.
        /// </summary>
        ITimeZone GetSystemTimeZone();

        /// <summary>
        /// Converts a <see cref="DateTimeOffset" /> to the specified <see cref="ITimeZone" /> instance.
        /// </summary>
        DateTimeOffset ConvertToTimeZone(DateTimeOffset dateTimeOffset, ITimeZone timeZone);
    }
}