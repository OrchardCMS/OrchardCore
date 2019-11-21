using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Handlers
{
    public class ExternallUserHandler : IExternallUserEventHandler
    {
        private readonly IWorkflowManager _workflowManager;

        public ExternallUserHandler(IWorkflowManager workflowManager)
        {
            _workflowManager = workflowManager;
        }

        public Task LoggedIn(ExternalUserContext context)
        {
            return TriggerWorkflowEventAsync(nameof(ExternalUserLoggedInEvent), (User)context.User);
        }

        private Task TriggerWorkflowEventAsync(string name, User user)
        {
            return _workflowManager.TriggerEventAsync(name,
                input: new { User = user },
                correlationId: user.Id.ToString()
            );
        }

        public Task ConfigureRoles(ExternalUserContext context)
        {
            return Task.CompletedTask;
        }

        public Task RequestUsername(ExternalUserContext context)
        {
            return Task.CompletedTask;
        }

    }
}
