using System.Threading.Tasks;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Handlers
{
    public class UserCreatedHandler : IUserCreatedEventHandler
    {
        private readonly IWorkflowManager _workflowManager;

        public UserCreatedHandler(IWorkflowManager workflowManager)
        {
            _workflowManager = workflowManager;
        }

        public Task CreatedAsync(CreateUserContext context)
        {
            return TriggerWorkflowEventAsync(nameof(UserCreatedEvent), (User)context.User);
        }

        private Task TriggerWorkflowEventAsync(string name, User user)
        {
            return _workflowManager.TriggerEventAsync(name,
                input: new { User = user },
                correlationId: user.Id.ToString()
            );
        }
    }
}
