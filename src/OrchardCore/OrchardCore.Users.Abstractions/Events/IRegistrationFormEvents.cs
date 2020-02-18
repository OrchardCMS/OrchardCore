using System;
using System.Threading.Tasks;

namespace OrchardCore.Users.Events
{
    /// <summary>
    /// Contract for user registration events.
    /// </summary>
    public interface IRegistrationFormEvents
    {
        /// <summary>
        /// Occurs during the user registration.
        /// </summary>
        /// <param name="reportError">The reported error if failure happened in validation process.</param>
        Task RegistrationValidationAsync(Action<string, string> reportError);

        /// <summary>
        /// Occurs when user has been registered.
        /// </summary>
        /// <param name="user">The <see cref="IUser"/></param>
        Task RegisteredAsync(IUser user);
    }
}
