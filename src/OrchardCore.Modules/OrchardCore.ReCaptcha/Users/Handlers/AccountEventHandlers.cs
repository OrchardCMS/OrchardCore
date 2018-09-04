using System.Threading;
using System.Threading.Tasks;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers
{
    public class AccountEventHandlers : IAccountEvents
    {
        public Task LoggedIn()
        {
            return Task.CompletedTask;
        }

        public Task LoggingIn(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task LoggingInFailed()
        {
            return Task.CompletedTask;
        }
    }
}
