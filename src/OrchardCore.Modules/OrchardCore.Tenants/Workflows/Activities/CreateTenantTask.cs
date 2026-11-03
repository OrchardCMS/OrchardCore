using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Localization;
using OrchardCore.Tenants.Models;
using OrchardCore.Tenants.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities;

public class CreateTenantTask : TenantTask, IActivityProvidedValues, IActivityDescription
{
    private readonly ITenantValidator _tenantValidator;

    public CreateTenantTask(
        IShellSettingsManager shellSettingsManager,
        IShellHost shellHost,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IWorkflowScriptEvaluator scriptEvaluator,
        ITenantValidator tenantValidator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<CreateTenantTask> localizer)
        : base(shellSettingsManager, shellHost, expressionEvaluator, scriptEvaluator, localizerFactory, localizer)
    {
        _tenantValidator = tenantValidator;
    }

    public override string Name => nameof(CreateTenantTask);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<CreateTenantTask>("Create Tenant Task");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);

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
        => Outcome(S["Done"], S["Failed"]);

    private static readonly ActivityProvidedValue[] s_providedValues = [ActivityProvidedValue.LastResult("any", LocalizationSource.Create<CreateTenantTask>("The settings of the tenant it created, or the validation errors when it failed."))];

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
        => s_providedValues;

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

        if (ShellHost.TryGetSettings(tenantName, out _))
        {
            return Outcome("Failed");
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

        // Apply the same validation as the admin UI and the API, so that a workflow can't create a tenant
        // whose name would be rejected there. The name is used as a folder name, e.g. to resolve the tenant
        // media root, so an unvalidated one can redirect tenant owned files outside of the tenant directory.
        var errors = await _tenantValidator.ValidateAsync(new TenantApiModel
        {
            Name = tenantName,
            Description = description,
            RequestUrlPrefix = requestUrlPrefix,
            RequestUrlHost = requestUrlHost,
            DatabaseProvider = databaseProvider,
            ConnectionString = connectionString,
            TablePrefix = tablePrefix,
            Schema = schema,
            RecipeName = recipeName,
            FeatureProfiles = featureProfile?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [],
            IsNewTenant = true,
        });

        if (errors.Any())
        {
            workflowContext.LastResult = errors;

            return Outcome("Failed");
        }

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

        return Outcome("Done");
    }
}
