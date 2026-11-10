using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities;

public abstract class TenantActivity : Activity
{
    protected readonly IStringLocalizer S;

    /// <summary>
    /// Gets the factory that translates the <see cref="LocalizationSource"/> descriptions of the activity.
    /// </summary>
    protected IStringLocalizerFactory LocalizerFactory { get; }

    protected TenantActivity(IShellSettingsManager shellSettingsManager, IShellHost shellHost, IWorkflowExpressionEvaluator expressionEvaluator, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizerFactory localizerFactory, IStringLocalizer localizer)
    {
        ShellSettingsManager = shellSettingsManager;
        ShellHost = shellHost;
        ExpressionEvaluator = expressionEvaluator;
        ScriptEvaluator = scriptEvaluator;
        LocalizerFactory = localizerFactory;
        S = localizer;
    }

    public WorkflowExpression<string> TenantName
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    protected IShellSettingsManager ShellSettingsManager { get; }
    protected IShellHost ShellHost { get; }
    protected IWorkflowExpressionEvaluator ExpressionEvaluator { get; }
    protected IWorkflowScriptEvaluator ScriptEvaluator { get; }

    public static LocalizationSource CategoryName => TenantsLocalizationSources.Tenant;

    public override LocalizedString Category => LocalizerFactory.Localize(CategoryName);

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcome(S["Done"]);
    }

    public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcome("Done");
    }
}
