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
        /// Occurs when the user is logging.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="reportError">The reported error if failure happened during the login process.</param>
        Task LoggingInAsync(string userName, Action<string, string> reportError);

        /// <summary>
        /// Occurs when the user login has failed and the user was not found.
        /// </summary>
        /// <param name="userName">The username.</param>
        Task LoggingInFailedAsync(string userName);

        /// <summary>
        /// Occurs when the user login has failed and the user is known.
        /// </summary>
        /// <param name="user">The <see cref="IUser"/>.</param>
        Task LoggingInFailedAsync(IUser user);

        /// <summary>
        /// Occurs when a user is locked out.
        /// </summary>
        /// <param name="user">The <see cref="IUser"/>.</param>
        Task IsLockedOutAsync(IUser user);

        /// <summary>
        /// Occurs when the user is logged in.
        /// </summary>
        /// <param name="user">The <see cref="IUser"/>.</param>
        Task LoggedInAsync(IUser user);
    }
}
