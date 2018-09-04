using System.Threading.Tasks;
using OrchardCore.Users.Events;

namespace OrchardCore.ReCaptcha.Users.Handlers
{
    public class RegistrationEventHandlers : IRegistrationEvents
    {
        public Task Registered()
        {
            return Task.CompletedTask;
        }

        public Task Registering()
        {
            return Task.CompletedTask;
        }
    }
}
