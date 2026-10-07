using Microsoft.Extensions.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public abstract class UserEvent : UserActivity, IEvent, IActivityProvidedValues
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
        return Outcome("Done");
    }

    public virtual IEnumerable<ActivityProvidedValue> GetProvidedValues()
        => [new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "User", TypeName = "any", Description = S["The user of the event."], Members = WorkflowValueMembers.User(S) }];
}
