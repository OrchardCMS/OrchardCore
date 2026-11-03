using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public class AssignUserRoleTask : TaskActivity<AssignUserRoleTask>, IActivityDescription
{
    private readonly UserManager<IUser> _userManager;
    private readonly IUserService _userService;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public AssignUserRoleTask(
        UserManager<IUser> userManager,
        IUserService userService,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<AssignUserRoleTask> localizer)
    {
        _userManager = userManager;
        _userService = userService;
        _expressionEvaluator = expressionEvaluator;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<AssignUserRoleTask>("Assign User Role Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.User;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

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
        => Outcome(S["Done"], S["Failed"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var userName = await _expressionEvaluator.EvaluateAsync(UserName, workflowContext, null);
        var roleName = await _expressionEvaluator.EvaluateAsync(RoleName, workflowContext, null);

        var user = (User)await _userService.GetUserAsync(userName);

        if (user != null)
        {
            if (!user.RoleNames.Contains(roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }

            return Outcome("Done");
        }
        else
        {
            return Outcome("Failed");
        }
    }
}
