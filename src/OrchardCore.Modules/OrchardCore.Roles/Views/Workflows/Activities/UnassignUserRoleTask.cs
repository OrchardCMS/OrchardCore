using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Roles.Workflows.Activities;

public class UnassignUserRoleTask : TaskActivity<UnassignUserRoleTask>
{
    private readonly UserManager<IUser> _userManager;
    private readonly IUserService _userService;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;

    public UnassignUserRoleTask(UserManager<IUser> userManager, IUserService userService, IWorkflowExpressionEvaluator expressionvaluator, IStringLocalizer<UnassignUserRoleTask> localizer)
    {
        _userManager = userManager;
        _userService = userService;
        _expressionEvaluator = expressionvaluator;
        S = localizer;
    }

    public override LocalizedString DisplayText => S["Unassign User Role Task"];

    public override LocalizedString Category => S["User"];

    public WorkflowExpression<string> UserName
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public IEnumerable<string> Roles
    {
        get => GetProperty(() => new List<string>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcomes(S["Done"], S["Failed"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var userName = await _expressionEvaluator.EvaluateAsync(UserName, workflowContext, null);

        var u = await _userService.GetUserAsync(userName);

        if (u is User user)
        {
            foreach (var role in Roles)
            {
                if (user.RoleNames.Contains(role))
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }
            }

            return Outcomes("Done");
        }

        return Outcomes("Failed");
    }
}
