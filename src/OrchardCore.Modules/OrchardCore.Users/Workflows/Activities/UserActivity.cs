using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public abstract class UserActivity : Activity
{
    protected readonly IStringLocalizer S;

    /// <summary>
    /// Gets the factory that translates the <see cref="LocalizationSource"/> descriptions of the activity.
    /// </summary>
    protected IStringLocalizerFactory LocalizerFactory { get; }

    protected UserActivity(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizerFactory localizerFactory, IStringLocalizer localizer)
    {
        UserService = userService;
        ScriptEvaluator = scriptEvaluator;
        LocalizerFactory = localizerFactory;
        S = localizer;
    }

    protected IUserService UserService { get; }

    protected IWorkflowScriptEvaluator ScriptEvaluator { get; }

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.User;

    public override LocalizedString Category => LocalizerFactory.Localize(CategoryName);

    /// <summary>
    /// An expression that evaluates to an <see cref="User"/> item.
    /// </summary>
    public WorkflowExpression<User> User
    {
        get => GetProperty(() => new WorkflowExpression<User>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcome(S["Done"]);
    }

    public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcome("Done");
    }
}
