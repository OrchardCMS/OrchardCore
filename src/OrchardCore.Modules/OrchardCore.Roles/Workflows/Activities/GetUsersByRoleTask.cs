using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Roles.Workflows.Activities;

public class GetUsersByRoleTask : TaskActivity<GetUsersByRoleTask>, IActivityProvidedValues, IActivityDescription
{
    private readonly UserManager<IUser> _userManager;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public GetUsersByRoleTask(UserManager<IUser> userManager, IWorkflowExpressionEvaluator expressionEvaluator, IStringLocalizerFactory localizerFactory, IStringLocalizer<GetUsersByRoleTask> localizer)
    {
        _userManager = userManager;
        _expressionEvaluator = expressionEvaluator;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<GetUsersByRoleTask>("Get Users by Role Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.User;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

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
        => Outcome(S["Done"], S["Failed"]);

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

            return Outcome("Done");
        }

        return Outcome("Failed");
    }

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
    {
        var name = OutputKeyName?.Expression?.Trim();

        // A name computed by Liquid is only known when the activity runs.
        return string.IsNullOrEmpty(name) || name.Contains("{{") || name.Contains("{%")
            ? []
            : [new ActivityProvidedValue { Source = WorkflowValueSource.Output, Name = name, TypeName = "object", Description = LocalizationSource.Create<GetUsersByRoleTask>("The users in the selected roles: their ids, by user name.") }];
    }
}
