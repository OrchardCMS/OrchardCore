using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities;

public class EnableTenantTask : TenantTask
{
    public EnableTenantTask(IShellSettingsManager shellSettingsManager, IShellHost shellHost, IWorkflowExpressionEvaluator expressionEvaluator, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<EnableTenantTask> localizer)
        : base(shellSettingsManager, shellHost, expressionEvaluator, scriptEvaluator, localizer)
    {
    }

    public override string Name => nameof(EnableTenantTask);

    public override LocalizedString Category => S["Tenant"];

    public override LocalizedString DisplayText => S["Enable Tenant Task"];

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcome(S["Enabled"], S["Failed"]);
    }

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

        if (!shellSettings.IsDisabled())
        {
            return Outcome("Failed");
        }

        await ShellHost.UpdateShellSettingsAsync(shellSettings.AsRunning());

        return Outcome("Enabled");
    }
}
