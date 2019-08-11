using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Users.Events
{
    public interface ILoginFormEvent
    {
        Task LoggingInAsync(Action<string, string> reportError);

        Task LoggingInFailedAsync();

        Task LoggedInAsync();
    }
}
