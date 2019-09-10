using System;
using System.Threading.Tasks;

namespace OrchardCore.Users.Events
{
    public interface IRegistrationFormEvents
    {
        Task RegistrationValidationAsync(Action<string, string> reportError);

        Task RegisteredAsync(IUser user);
    }
}