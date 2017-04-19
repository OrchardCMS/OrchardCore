using System;

namespace Microsoft.AspNetCore.Modules
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
		/// </summary>
		DateTime UtcNow { get; }
    }
}