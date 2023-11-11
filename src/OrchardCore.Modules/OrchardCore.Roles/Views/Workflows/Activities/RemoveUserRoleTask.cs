using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Roles.Workflows.Activities
{
    public class RemoveUserRoleTask : TaskActivity
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IUserService _userService;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        protected readonly IStringLocalizer S;

        public RemoveUserRoleTask(UserManager<IUser> userManager, IUserService userService, IWorkflowExpressionEvaluator expressionvaluator, IStringLocalizer<RemoveUserRoleTask> localizer)
        {
            _userManager = userManager;
            _userService = userService;
            _expressionEvaluator = expressionvaluator;
            S = localizer;
        }

        public override string Name => nameof(RemoveUserRoleTask);

        public override LocalizedString DisplayText => S["Remove User Role Task"];

        public override LocalizedString Category => S["User"];

        public WorkflowExpression<string> UserName
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
            var userName = await _expressionEvaluator.EvaluateAsync(UserName, workflowContext, null);
            var roleName = await _expressionEvaluator.EvaluateAsync(RoleName, workflowContext, null);

            var user = (User)await _userService.GetUserAsync(userName);

            if (user != null)
            {
                if (user.RoleNames.Contains(roleName))
                {
                    await _userManager.RemoveFromRoleAsync(user, roleName);
                }

                return Outcomes("Done");
            }
            else
            {
                return Outcomes("Failed");
            }
        }
    }
}
