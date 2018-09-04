using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Users.Events
{
    public interface IAccountEvents
    {
        Task LoggingIn(CancellationToken token);

        Task LoggingInFailed();

        Task LoggedIn();
    }
}
