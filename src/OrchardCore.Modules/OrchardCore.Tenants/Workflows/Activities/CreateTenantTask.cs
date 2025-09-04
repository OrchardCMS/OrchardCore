using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities;

public class CreateTenantTask : TenantTask
{
    public CreateTenantTask(
        IShellSettingsManager shellSettingsManager,
        IShellHost shellHost,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizer<CreateTenantTask> localizer)
        : base(shellSettingsManager, shellHost, expressionEvaluator, scriptEvaluator, localizer)
    {
    }

    public override string Name => nameof(CreateTenantTask);

    public override LocalizedString Category => S["Tenant"];

    public override LocalizedString DisplayText => S["Create Tenant Task"];

    public WorkflowExpression<string> Description
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> RequestUrlPrefix
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> RequestUrlHost
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> DatabaseProvider
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> ConnectionString
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> TablePrefix
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> Schema
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> RecipeName
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> FeatureProfile
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Done"], S["Failed"]);
    }

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (!ShellScope.Context.Settings.IsDefaultShell())
        {
            return Outcomes("Failed");
        }

        var tenantName = (await ExpressionEvaluator.EvaluateAsync(TenantName, workflowContext, null))?.Trim();

        if (string.IsNullOrEmpty(tenantName))
        {
            return Outcomes("Failed");
        }

        if (ShellHost.TryGetSettings(tenantName, out _))
        {
            return Outcomes("Failed");
        }

        var description = (await ExpressionEvaluator.EvaluateAsync(Description, workflowContext, null))?.Trim();
        var requestUrlPrefix = (await ExpressionEvaluator.EvaluateAsync(RequestUrlPrefix, workflowContext, null))?.Trim();
        var requestUrlHost = (await ExpressionEvaluator.EvaluateAsync(RequestUrlHost, workflowContext, null))?.Trim();
        var databaseProvider = (await ExpressionEvaluator.EvaluateAsync(DatabaseProvider, workflowContext, null))?.Trim();
        var connectionString = (await ExpressionEvaluator.EvaluateAsync(ConnectionString, workflowContext, null))?.Trim();
        var tablePrefix = (await ExpressionEvaluator.EvaluateAsync(TablePrefix, workflowContext, null))?.Trim();
        var schema = (await ExpressionEvaluator.EvaluateAsync(Schema, workflowContext, null))?.Trim();
        var recipeName = (await ExpressionEvaluator.EvaluateAsync(RecipeName, workflowContext, null))?.Trim();
        var featureProfile = (await ExpressionEvaluator.EvaluateAsync(FeatureProfile, workflowContext, null))?.Trim();

        // Creates a default shell settings based on the configuration.
        using var shellSettings = ShellSettingsManager
            .CreateDefaultSettings()
            .AsUninitialized()
            .AsDisposable();

        shellSettings.Name = tenantName;

        if (!string.IsNullOrEmpty(description))
        {
            shellSettings["Description"] = description;
        }

        if (!string.IsNullOrEmpty(requestUrlHost))
        {
            shellSettings.RequestUrlHost = requestUrlHost;
        }

        if (!string.IsNullOrEmpty(requestUrlPrefix))
        {
            shellSettings.RequestUrlPrefix = requestUrlPrefix;
        }

        shellSettings.AsUninitialized();

        if (!string.IsNullOrEmpty(connectionString))
        {
            shellSettings["ConnectionString"] = connectionString;
        }

        if (!string.IsNullOrEmpty(tablePrefix))
        {
            shellSettings["TablePrefix"] = tablePrefix;
        }

        if (!string.IsNullOrEmpty(schema))
        {
            shellSettings["Schema"] = schema;
        }

        if (!string.IsNullOrEmpty(databaseProvider))
        {
            shellSettings["DatabaseProvider"] = databaseProvider;
        }

        if (!string.IsNullOrEmpty(recipeName))
        {
            shellSettings["RecipeName"] = recipeName;
        }

        if (!string.IsNullOrEmpty(featureProfile))
        {
            shellSettings["FeatureProfile"] = featureProfile;
        }

        shellSettings["Secret"] = Guid.NewGuid().ToString();

        await ShellHost.UpdateShellSettingsAsync(shellSettings);
        var reloadedSettings = ShellHost.GetSettings(shellSettings.Name);

        workflowContext.LastResult = reloadedSettings;
        workflowContext.CorrelationId = reloadedSettings.Name;

        return Outcomes("Done");
    }
}
