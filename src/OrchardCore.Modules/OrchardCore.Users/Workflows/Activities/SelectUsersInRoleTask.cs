using System;
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

        public WorkflowExpression<string> PropertyName
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
            return Outcomes(S["Done"], S["Empty"], S["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var propName = await _expressionEvaluator.EvaluateAsync(PropertyName, workflowContext, null);
            var roleName = await _expressionEvaluator.EvaluateAsync(RoleName, workflowContext, null);

            if (!string.IsNullOrEmpty(propName) && !string.IsNullOrEmpty(roleName))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                if (usersInRole.Count > 0)
                {
                    List<string> output;
                    if (propName.Contains("email", StringComparison.InvariantCultureIgnoreCase))
                    {
                        output = usersInRole.Select((u) => (u as User)?.Email).ToList();
                    }
                    else
                    {
                        output = usersInRole.Select(u => u.UserName).ToList();
                    }
                    workflowContext.Properties[propName] = string.Join(",", output);
                    return Outcomes("Done");
                }
                return Outcomes("Empty");
            }
            return Outcomes("Failed");
        }
    }
}
