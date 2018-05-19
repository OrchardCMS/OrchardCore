using System;
using System.Threading.Tasks;

namespace OrchardCore.Modules
{
    /// <summary>
    /// Provides local values of the current time and timezone.
    /// </summary>
    public interface ILocalClock
    {
        /// <summary>
        /// Gets the time for the local timezone.
        /// </summary>
        Task<DateTimeOffset> LocalNowAsync { get; }

        /// <summary>
        /// Returns the local timezone.
        /// </summary>
        Task<ITimeZone> GetLocalTimeZoneAsync();

        /// <summary>
        /// Converts a <see cref="DateTimeOffset" /> to the specified <see cref="ITimeZone" /> instance.
        /// </summary>
        Task<DateTimeOffset> ConvertToLocalAsync(DateTimeOffset dateTimeOffset);
    }
}
