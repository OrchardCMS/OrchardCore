using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Users.Events
{
    public interface IForgotPasswordEvents
    {
        Task ResettingPassword();

        Task PasswordReset();

        Task ForgettingPassword();

        Task ForgotPassword();
    }
}
