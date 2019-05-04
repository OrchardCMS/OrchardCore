using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public class AssignUserRoleTask : TaskActivity
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IUserService _userService;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public AssignUserRoleTask(UserManager<IUser> userManager, IUserService userService, IWorkflowExpressionEvaluator expressionvaluator, IStringLocalizer<AssignUserRoleTask> t)
        {
            _userManager = userManager;
            _userService = userService;
            _expressionEvaluator = expressionvaluator;
            T = t;
        }

        private IStringLocalizer T { get; set; }
        public override string Name => nameof(AssignUserRoleTask);
        public override LocalizedString Category => T["User"];

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
            return Outcomes(T["Done"], T["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var userName = await _expressionEvaluator.EvaluateAsync(UserName, workflowContext);
            var roleName = await _expressionEvaluator.EvaluateAsync(RoleName, workflowContext);

            User user = (User)await _userService.GetUserAsync(userName);

            if (user != null)
            {
                if (!user.RoleNames.Contains(roleName))
                {
                    await _userManager.AddToRoleAsync(user, roleName);
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