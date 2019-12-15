using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Users.Events
{
    /// <summary>
    /// Contract for password recovery events.
    /// </summary>
    public interface IPasswordRecoveryFormEvents
    {
        /// <summary>
        /// Occures during the user password resetting.
        /// </summary>
        /// <param name="reportError">The reported error if fauilar happened during the ressting process.</param>
        Task ResettingPasswordAsync(Action<string, string> reportError);

        /// <summary>
        /// Occures after the user password has been reset.
        /// </summary>
        Task PasswordResetAsync();

        /// <summary>
        /// Occures during the user password recovery.
        /// </summary>
        /// <param name="reportError">The reported error if fauilar happened during the recovery process.</param>
        Task RecoveringPasswordAsync(Action<string, string> reportError);

        /// <summary>
        /// Occures after the user password has been recovered.
        /// </summary>
        Task PasswordRecoveredAsync();
    }
}
