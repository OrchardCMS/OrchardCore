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
        /// Gets the time for the loca timezone.
        /// </summary>
        Task<DateTimeOffset> LocalNowAsync { get; }

        /// <summary>
        /// Returns the local timezone.
        /// </summary>
        Task<ITimeZone> GetLocalTimeZoneAsync();
    }
}
