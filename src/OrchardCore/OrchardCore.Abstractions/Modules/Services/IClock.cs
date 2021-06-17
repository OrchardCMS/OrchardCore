using System;

namespace OrchardCore.Modules
{
    /// <summary>
    /// Provides the current UTC <see cref="DateTime"/>, and timezone related methods.
    /// This service should be used whenever the current date and time are needed, instead of <seealso cref="DateTime"/> directly.
    /// If local date time and timezones are needed use <see cref="ILocalClock" /> instead.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the current <see cref="DateTime"/> of the system, expressed in UTC.
        /// </summary>
        /// <remarks>
        /// A <see cref="DateTime"/> as this property is usually used to store the current date time in UTC and a <see cref="DateTimeOffset" />
        /// would affect usability.
        /// </remarks>
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
