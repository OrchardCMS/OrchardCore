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

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Actions.Select(x => Outcome(T[x]));
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var action = (string)workflowContext.Input["UserAction"];

            return Outcomes(action);
        }
    }
}