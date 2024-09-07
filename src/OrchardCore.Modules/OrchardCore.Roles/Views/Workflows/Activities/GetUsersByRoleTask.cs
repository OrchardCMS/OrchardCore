using Fluid;
using Fluid.Values;
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

    public GetUsersByRoleTask(UserManager<IUser> userManager, IWorkflowExpressionEvaluator expressionEvaluator, IStringLocalizer<GetUsersByRoleTask> localizer)
    {
        _userManager = userManager;
        _expressionEvaluator = expressionEvaluator;
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
            var usersInRole = new Dictionary<string, string>();

            foreach (var role in Roles)
            {
                foreach (var u in await _userManager.GetUsersInRoleAsync(role))
                {
                    if (u is not User user)
                    {
                        continue;
                    }

                    usersInRole.TryAdd(user.Id.ToString(), user.UserId);
                }
            }

            workflowContext.Output[outputKeyName] = FluidValue.Create(usersInRole, new TemplateOptions());

            return Outcomes("Done");
        }

        return Outcomes("Failed");
    }
}
