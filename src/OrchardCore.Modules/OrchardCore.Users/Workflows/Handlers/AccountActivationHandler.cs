using System;
using System.Threading.Tasks;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Handlers
{
    public class AccountActivationHandler : IAccountActivationEventHandler
    {
        private readonly IWorkflowManager _workflowManager;

        public AccountActivationHandler(IWorkflowManager workflowManager)
        {
            _workflowManager = workflowManager;
        }

        public Task AccountActivationEventHandler(AccountActivationContext context)
        {
            return TriggerWorkflowEventAsync(nameof(AccountActivationEvent), context);
        }

        private Task TriggerWorkflowEventAsync(string name, AccountActivationContext context)
        {
            var user = context.User as User;

            return _workflowManager.TriggerEventAsync(name,
                input: new { Context = context },
                correlationId: user.Id.ToString()
            );
        }
    }
}
