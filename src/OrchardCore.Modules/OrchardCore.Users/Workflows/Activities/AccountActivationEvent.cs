using Microsoft.Extensions.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities
{
    public class AccountActivationEvent : UserEvent, IEvent
    {
        public AccountActivationEvent(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<AccountActivationEvent> localizer) : base(userService, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(AccountActivationEvent);

        public string PropertyName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public override LocalizedString DisplayText => T["Account Activation Event"];

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