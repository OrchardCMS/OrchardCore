using System;
using System.Threading.Tasks;

namespace OrchardCore.Users.Events
{
    /// <summary>
    /// Contract for password recovery events.
    /// </summary>
    public interface IPasswordRecoveryFormEvents
    {
        /// <summary>
        /// Occurs during the user password is being reset.
        /// </summary>
        /// <param name="reportError">The reported error if failure happened during the rest process.</param>
        Task ResettingPasswordAsync(Action<string, string> reportError);

        /// <summary>
        /// Occurs after the user password has been reset.
        /// </summary>
        Task PasswordResetAsync(PasswordRecoveryContext context);

        /// <summary>
        /// Occurs during the user password recovery.
        /// </summary>
        /// <param name="reportError">The reported error if failure happened during the recovery process.</param>
        Task RecoveringPasswordAsync(Action<string, string> reportError);

        /// <summary>
        /// Occurs after the user password has been recovered.
        /// </summary>
        Task PasswordRecoveredAsync(PasswordRecoveryContext context);
    }
}
