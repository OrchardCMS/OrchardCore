using System;
using System.Threading.Tasks;

namespace OrchardCore.Users.Events
{
    /// <summary>
    /// Contract for login events.
    /// </summary>
    public interface ILoginFormEvent
    {
        /// <summary>
        /// Occurs when the user is login.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="reportError">The reported error if fauilar happened during the login process.</param>
        Task LoggingInAsync(string userName, Action<string, string> reportError);

        /// <summary>
        /// Occurs when the user login is failed.
        /// </summary>
        /// <param name="userName">The username.</param>
        Task LoggingInFailedAsync(string userName);

        /// <summary>
        /// Occurs when the user is logged in.
        /// </summary>
        /// <param name="userName">The username.</param>
        Task LoggedInAsync(string userName);
    }
}
