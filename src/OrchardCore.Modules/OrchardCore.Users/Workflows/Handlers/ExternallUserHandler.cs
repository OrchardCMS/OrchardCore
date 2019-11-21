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
    public class ExternallUserHandler : IProvideExternalUserRolesEventHandler
    {
        private readonly IWorkflowManager _workflowManager;

        public ExternallUserHandler(IWorkflowManager workflowManager)
        {
            _workflowManager = workflowManager;
        }

        public Task UpdateRoles(UpdateRolesContext context)
        {
            return _workflowManager.TriggerEventAsync(nameof(ExternalUserLoggedInEvent),
                input: new { context.User, context.Claims, context.CurrentRoles },
                correlationId: ((User)context.User).Id.ToString()
            );
        }
    }
}
