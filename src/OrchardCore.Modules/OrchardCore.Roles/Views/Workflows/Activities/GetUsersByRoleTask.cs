using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Roles.Workflows.Activities;

public class GetUsersByRoleTask : TaskActivity
{
    private readonly UserManager<IUser> _userManager;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;

    public GetUsersByRoleTask(UserManager<IUser> userManager, IWorkflowExpressionEvaluator expressionvaluator, IStringLocalizer<GetUsersByRoleTask> localizer)
    {
        _userManager = userManager;
        _expressionEvaluator = expressionvaluator;
        S = localizer;
    }

    public override string Name => nameof(GetUsersByRoleTask);

    public override LocalizedString DisplayText => S["Get Users by Role Task"];

    public override LocalizedString Category => S["User"];

    public WorkflowExpression<string> OutputKeyName
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
    {
        return Outcomes(S["Done"], S["Failed"]);
    }

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var propKeyName = await _expressionEvaluator.EvaluateAsync(OutputKeyName, workflowContext, null);

        if (!string.IsNullOrEmpty(propKeyName))
        {
            if (Roles.Any())
            {
                HashSet<IUser> usersInRole = new HashSet<IUser>();
                foreach (var role in Roles)
                {
                    usersInRole.UnionWith(await _userManager.GetUsersInRoleAsync(role));
                }
                if (usersInRole.Any())
                {
                    workflowContext.Output[propKeyName] = usersInRole.Select(u => (u as User).UserId).ToArray();
                    return Outcomes("Done");
                }
            }
        }
        return Outcomes("Failed");
    }
}
