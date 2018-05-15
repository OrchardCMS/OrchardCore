using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.UserTasks.Activities
{
    public class UserTaskEvent : EventActivity
    {
        public UserTaskEvent(IStringLocalizer<UserTaskEvent> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(UserTaskEvent);
        public override LocalizedString Category => T["Content"];

        public IList<string> Actions
        {
            get => GetProperty(() => new List<string>());
            set => SetProperty(value);
        }

        public IList<string> Roles
        {
            get => GetProperty(() => new List<string>());
            set => SetProperty(value);
        }

        public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var action = GetProvidedAction(workflowContext);
            return Actions.Contains(action);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Actions.Select(x => Outcome(T[x]));
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var action = GetProvidedAction(workflowContext);
            return Outcomes(action);
        }

        private string GetProvidedAction(WorkflowExecutionContext workflowContext)
        {
            return (string)workflowContext.Input["UserAction"];
        }
    }
}