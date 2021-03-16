using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities
{
    public abstract class UserActivity : Activity
    {
        protected readonly IStringLocalizer S;

        protected UserActivity(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer localizer)
        {
            UserService = userService;
            ScriptEvaluator = scriptEvaluator;
            S = localizer;
        }

        protected IUserService UserService { get; }

        protected IWorkflowScriptEvaluator ScriptEvaluator { get; }

        public override LocalizedString Category => S["User"];

        /// <summary>
        /// An expression that evaluates to an <see cref="User"/> item.
        /// </summary>
        public WorkflowExpression<User> User
        {
            get => GetProperty(() => new WorkflowExpression<User>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Done");
        }
    }
}
