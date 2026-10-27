using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities;

public class DisableTenantTask : TenantTask, IActivityDescription
{
    public DisableTenantTask(IShellSettingsManager shellSettingsManager, IShellHost shellHost, IWorkflowExpressionEvaluator expressionEvaluator, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizerFactory localizerFactory, IStringLocalizer<DisableTenantTask> localizer)
        : base(shellSettingsManager, shellHost, expressionEvaluator, scriptEvaluator, localizerFactory, localizer)
    {
    }
    public override string Name => nameof(DisableTenantTask);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<DisableTenantTask>("Disable Tenant Task");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Disabled"], S["Failed"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (!ShellScope.Context.Settings.IsDefaultShell())
        {
            return Outcome("Failed");
        }

        var tenantName = (await ExpressionEvaluator.EvaluateAsync(TenantName, workflowContext, null))?.Trim();

        if (string.IsNullOrEmpty(tenantName))
        {
            return Outcome("Failed");
        }

        if (!ShellHost.TryGetSettings(tenantName, out var shellSettings))
        {
            return Outcome("Failed");
        }

        if (shellSettings.IsDefaultShell())
        {
            return Outcome("Failed");
        }

        if (!shellSettings.IsRunning())
        {
            return Outcome("Failed");
        }

        await ShellHost.UpdateShellSettingsAsync(shellSettings.AsDisabled());

        return Outcome("Disabled");
    }
}
