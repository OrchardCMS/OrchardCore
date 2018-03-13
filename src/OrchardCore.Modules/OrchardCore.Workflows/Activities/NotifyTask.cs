using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class NotifyTask : TaskActivity
    {
        private readonly INotifier _notifier;
        private readonly IStringLocalizer S;

        public NotifyTask(INotifier notifier, IStringLocalizer<NotifyTask> s)
        {
            _notifier = notifier;

            S = s;
        }

        public override string Name => nameof(NotifyTask);
        public override LocalizedString Category => S["UI"];

        public NotifyType NotificationType
        {
            get => GetProperty<NotifyType>();
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Message
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var message = await workflowContext.EvaluateExpressionAsync(Message);
            _notifier.Add(NotificationType, new LocalizedHtmlString(nameof(NotifyTask), message));

            return Outcomes("Done");
        }
    }
}