using System;
using System.Threading.Tasks;

namespace OrchardCore.Modules
{
    /// <summary>
    /// Provides local values of the current time and time zone.
    /// </summary>
    public interface ILocalClock
    {
        /// <summary>
        /// Gets the time for the local time zone.
        /// </summary>
        Task<DateTimeOffset> LocalNowAsync { get; }

        /// <summary>
        /// Returns the local time zone.
        /// </summary>
        Task<ITimeZone> GetLocalTimeZoneAsync();

        /// <summary>
        /// Converts a <see cref="DateTimeOffset" /> to the specified <see cref="ITimeZone" /> instance.
        /// </summary>
        Task<DateTimeOffset> ConvertToLocalAsync(DateTimeOffset dateTimeOffset);

        /// <summary>
        /// Converts a <see cref="DateTime" /> representing a local time to the UTC value.
        /// </summary>
        Task<DateTime> ConvertToUtcAsync(DateTime dateTime);
    }
}
