using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Users.Events
{
    public interface IPasswordRecoveryFormEvents
    {
        Task ResettingPasswordAsync(Action<string, string> reportError);

        Task PasswordResetAsync();

        Task RecoveringPasswordAsync(Action<string, string> reportError);

        Task PasswordRecoveredAsync();
    }
}
