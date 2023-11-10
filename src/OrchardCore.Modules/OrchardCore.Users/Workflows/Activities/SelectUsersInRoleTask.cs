using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities
{
    public class SelectUsersInRoleTask : TaskActivity
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IUserService _userService;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        protected readonly IStringLocalizer S;

        public SelectUsersInRoleTask(UserManager<IUser> userManager, IUserService userService, IWorkflowExpressionEvaluator expressionvaluator, IStringLocalizer<SelectUsersInRoleTask> localizer)
        {
            _userManager = userManager;
            _userService = userService;
            _expressionEvaluator = expressionvaluator;
            S = localizer;
        }

        public override string Name => nameof(SelectUsersInRoleTask);

        public override LocalizedString DisplayText => S["Select Users in Role Task"];

        public override LocalizedString Category => S["User"];

        public WorkflowExpression<string> OutputKeyName
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> RoleName
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var propKeyName = await _expressionEvaluator.EvaluateAsync(OutputKeyName, workflowContext, null);
            var roleName = await _expressionEvaluator.EvaluateAsync(RoleName, workflowContext, null);

            if (!string.IsNullOrEmpty(propKeyName) && !string.IsNullOrEmpty(roleName))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                if (usersInRole.Count > 0)
                {
                    workflowContext.Output[propKeyName] = usersInRole.Select(u => (u as User).UserId).ToArray();
                    return Outcomes("Done");
                }
            }
            return Outcomes("Failed");
        }
    }
}
