using System;
using System.Threading.Tasks;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Handlers
{
    public class AccountActivatedHandler : IAccountActivatedEventHandler
    {
        private readonly IWorkflowManager _workflowManager;

        public AccountActivatedHandler(IWorkflowManager workflowManager)
        {
            _workflowManager = workflowManager;
        }

        public Task AccountActivatedAsync(AccountActivatedContext context)
        {
            return TriggerWorkflowEventAsync(nameof(AccountActivatedEvent), context);
        }

        private Task TriggerWorkflowEventAsync(string name, AccountActivatedContext context)
        {
            var user = context.User as User;

            return _workflowManager.TriggerEventAsync(name,
                input: new { Context = context },
                correlationId: user.Id.ToString()
            );
        }
    }
}
