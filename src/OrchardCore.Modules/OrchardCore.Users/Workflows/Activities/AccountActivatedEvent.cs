using Microsoft.Extensions.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities
{
    public class AccountActivatedEvent : UserEvent, IEvent
    {
        public AccountActivatedEvent(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<UserLoggedInEvent> localizer) : base(userService, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(AccountActivatedEvent);

        public string PropertyName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public override LocalizedString DisplayText => T["Account Activated Event"];

        public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return !string.IsNullOrEmpty(PropertyName);
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            workflowContext.Properties[PropertyName] = workflowContext.Input["Context"];
            return Outcomes("Done");
        }
    }
}