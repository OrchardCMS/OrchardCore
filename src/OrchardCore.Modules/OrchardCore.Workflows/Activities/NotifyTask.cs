using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class NotifyTask : TaskActivity
    {
        private readonly INotifier _notifier;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public NotifyTask(INotifier notifier, IWorkflowExpressionEvaluator expressionvaluator, IStringLocalizer<NotifyTask> t)
        {
            _notifier = notifier;
            _expressionEvaluator = expressionvaluator;
            T = t;
        }
        
        private IStringLocalizer T { get; set; }
        public override string Name => nameof(NotifyTask);
        public override LocalizedString Category => T["UI"];

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
            return Outcomes(T["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var message = await _expressionEvaluator.EvaluateAsync(Message, workflowContext);
            _notifier.Add(NotificationType, new LocalizedHtmlString(nameof(NotifyTask), message));

            return Outcomes("Done");
        }
    }
}