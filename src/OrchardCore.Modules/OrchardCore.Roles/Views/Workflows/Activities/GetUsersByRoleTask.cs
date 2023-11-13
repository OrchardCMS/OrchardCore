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

public class GetUsersByRoleTask : TaskActivity<GetUsersByRoleTask>
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
        => Outcomes(S["Done"], S["Failed"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var outputKeyName = await _expressionEvaluator.EvaluateAsync(OutputKeyName, workflowContext, null);

        if (!string.IsNullOrEmpty(outputKeyName))
        {
            var usersInRole = new Dictionary<string, User>();

            foreach (var role in Roles)
            {
                foreach(var u in await _userManager.GetUsersInRoleAsync(role))
                {
                    if (u is not User user) 
                    {
                        continue;
                    }

                    usersInRole.TryAdd(user.UserId, user);
                }
            }
            
            workflowContext.Output[outputKeyName] = usersInRole.Values;

            return Outcomes("Done");
        }

        return Outcomes("Failed");
    }
}
