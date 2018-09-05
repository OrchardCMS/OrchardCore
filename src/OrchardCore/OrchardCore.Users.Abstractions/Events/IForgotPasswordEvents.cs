using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Users.Events
{
    public interface IForgotPasswordEvents
    {
        Task ResettingPasswordAsync(Action<string, string> reportError);

        Task PasswordResetAsync();

        Task ForgettingPasswordAsync(Action<string, string> reportError);

        Task ForgotPasswordAsync();
    }
}
