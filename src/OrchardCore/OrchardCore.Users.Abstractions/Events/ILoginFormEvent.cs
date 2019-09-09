using System;
using System.Threading.Tasks;

namespace OrchardCore.Users.Events
{
    public interface ILoginFormEvent
    {
        Task LoggingInAsync(string userName, Action<string, string> reportError);

        Task LoggingInFailedAsync(string userName);

        Task LoggedInAsync(string userName);
    }
}
