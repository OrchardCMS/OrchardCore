using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities
{
    public abstract class UserEvent : UserActivity, IEvent
    {
        public UserEvent(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer localizer) : base(userService, scriptEvaluator, localizer)
        {
        }
                
        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Halt();
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Done");
        }
    }
}