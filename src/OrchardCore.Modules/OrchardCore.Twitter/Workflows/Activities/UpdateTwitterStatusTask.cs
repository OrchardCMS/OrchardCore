using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
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
        private readonly IStringLocalizer T;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public UpdateTwitterStatusTask(TwitterClient twitterClient, IWorkflowExpressionEvaluator expressionEvaluator, IHttpContextAccessor httpContextAccessor, IUpdateModelAccessor updateModelAccessor, IStringLocalizer<UpdateTwitterStatusTask> t)
        {
            _twitterClient = twitterClient;
            _expressionEvaluator = expressionEvaluator;
            _httpContextAccessor = httpContextAccessor;
            _updateModelAccessor = updateModelAccessor;
            T = t;
        }

        // The technical name of the activity. Activities on a workflow definition reference this name.
        public override string Name => nameof(UpdateTwitterStatusTask);

        // The category to which this activity belongs. The activity picker groups activities by this category.
        public override LocalizedString Category => T["Social"];

        // The message to display.
        public WorkflowExpression<string> StatusTemplate
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        // Returns the possible outcomes of this activity.
        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"], T["Failed"]);
        }

        // This is the heart of the activity and actually performs the work to be done.
        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var status = await _expressionEvaluator.EvaluateAsync(StatusTemplate, workflowContext);

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
