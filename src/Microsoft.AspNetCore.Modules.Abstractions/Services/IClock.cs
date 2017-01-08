using System;

namespace Microsoft.AspNetCore.Modules
{
    /// <summary>
    /// Provides the current Utc <see cref="DateTimeOffset"/>, and time related method for cache management.
    /// This service should be used whenever the current date and time are needed, instead of <seealso cref="DateTimeOffset"/> directly.
    /// It also makes implementations more testable, as time can be mocked.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the current <see cref="DateTimeOffset"/> of the system, expressed in Utc
        /// </summary>
        DateTimeOffset UtcNow { get; }
    }
}