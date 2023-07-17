using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Twitter.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Twitter.Workflows.Activities
{
    public class UpdateTwitterStatusTask : TaskActivity
    {
        private readonly TwitterClient _twitterClient;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        protected readonly IStringLocalizer S;

        public UpdateTwitterStatusTask(
            TwitterClient twitterClient,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IStringLocalizer<UpdateTwitterStatusTask> localizer
            )
        {
            _twitterClient = twitterClient;
            _expressionEvaluator = expressionEvaluator;
            S = localizer;
        }

        // The technical name of the activity. Activities on a workflow definition reference this name.
        public override string Name => nameof(UpdateTwitterStatusTask);

        public override LocalizedString DisplayText => S["Update Twitter Status Task"];

        // The category to which this activity belongs. The activity picker groups activities by this category.
        public override LocalizedString Category => S["Social"];

        // The message to display.
        public WorkflowExpression<string> StatusTemplate
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        // Returns the possible outcomes of this activity.
        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Failed"]);
        }

        // This is the heart of the activity and actually performs the work to be done.
        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            // The twitter client encodes the status using FormUrlEncodedContent
            var status = await _expressionEvaluator.EvaluateAsync(StatusTemplate, workflowContext, null);

            var result = await _twitterClient.UpdateStatus(status);
            workflowContext.Properties.Add("TwitterResponse", await result.Content.ReadAsStringAsync());

            if (!result.IsSuccessStatusCode)
            {
                return Outcomes("Failed");
            }

            return Outcomes("Done");
        }
    }
}
