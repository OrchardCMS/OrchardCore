using System.Threading.Tasks;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Handlers
{
    public class UserEventHandler : IUserEventHandler
    {
        private readonly IWorkflowManager _workflowManager;

        public UserEventHandler(IWorkflowManager workflowManager)
        {
            _workflowManager = workflowManager;
        }

        public Task CreatedAsync(UserContext context)
        {
            return TriggerWorkflowEventAsync(nameof(UserCreatedEvent), (User)context.User);
        }

        public Task DisabledAsync(UserContext context)
        {
            return TriggerWorkflowEventAsync(nameof(UserDisabledEvent), (User)context.User);
        }

        public Task EnabledAsync(UserContext context)
        {
            return TriggerWorkflowEventAsync(nameof(UserEnabledEvent), (User)context.User);
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
