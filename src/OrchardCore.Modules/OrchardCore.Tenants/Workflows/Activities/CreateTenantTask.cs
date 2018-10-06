using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities
{
    public class CreateTenantTask : TenantTask
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public CreateTenantTask(IShellSettingsManager shellSettingsManager, IShellHost shellHost, IWorkflowExpressionEvaluator expressionEvaluator, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<CreateTenantTask> localizer)
            : base(shellSettingsManager, shellHost, scriptEvaluator, localizer)
        {
            _expressionEvaluator = expressionEvaluator;
        }

        public override string Name => nameof(CreateTenantTask);
        public override LocalizedString Category => T["Tenant"];

        public string ContentType
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
        
        public WorkflowExpression<string> RequestUrlPrefix
        {
            get => GetProperty<WorkflowExpression<string>>();
            set => SetProperty(value);
        }

        public WorkflowExpression<string> RequestUrlHost
        {
            get => GetProperty<WorkflowExpression<string>>();
            set => SetProperty(value);
        }

        public WorkflowExpression<string> DatabaseProvider
        {
            get => GetProperty<WorkflowExpression<string>>();
            set => SetProperty(value);
        }

        public WorkflowExpression<string> ConnectionString
        {
            get => GetProperty<WorkflowExpression<string>>();
            set => SetProperty(value);
        }

        public WorkflowExpression<string> TablePrefix
        {
            get => GetProperty<WorkflowExpression<string>>();
            set => SetProperty(value);
        }

        public WorkflowExpression<string> RecipeName
        {
            get => GetProperty<WorkflowExpression<string>>();
            set => SetProperty(value);
        }

        //public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        //{
        //    return !string.IsNullOrEmpty(TenantName);
        //}

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public async override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var requestUrlPrefixTask = _expressionEvaluator.EvaluateAsync(RequestUrlPrefix, workflowContext);
            var requestUrlHostTask = _expressionEvaluator.EvaluateAsync(RequestUrlHost, workflowContext);
            var databaseProviderTask = _expressionEvaluator.EvaluateAsync(DatabaseProvider, workflowContext);
            var connectionStringTask = _expressionEvaluator.EvaluateAsync(ConnectionString, workflowContext);
            var tablePrefixTask = _expressionEvaluator.EvaluateAsync(TablePrefix, workflowContext);
            var recipeNameTask = _expressionEvaluator.EvaluateAsync(RecipeName, workflowContext);

            await Task.WhenAll(requestUrlPrefixTask, requestUrlHostTask, databaseProviderTask, connectionStringTask, tablePrefixTask, recipeNameTask);

            var shellSettings = new ShellSettings();

            if (!string.IsNullOrWhiteSpace(TenantName))
            {
                shellSettings = new ShellSettings
                {
                    Name = TenantName,
                    RequestUrlPrefix = requestUrlPrefixTask.Result?.Trim(),
                    RequestUrlHost = requestUrlHostTask.Result?.Trim(),
                    ConnectionString = connectionStringTask.Result?.Trim(),
                    TablePrefix = tablePrefixTask.Result?.Trim(),
                    DatabaseProvider = databaseProviderTask.Result?.Trim(),
                    State = TenantState.Uninitialized,
                    Secret = Guid.NewGuid().ToString(),
                    RecipeName = recipeNameTask.Result.Trim()
                };
            }
            
            ShellSettingsManager.SaveSettings(shellSettings);
            var shellContext = await ShellHost.GetOrCreateShellContextAsync(shellSettings);

            workflowContext.LastResult = shellSettings;
            workflowContext.CorrelationId = shellSettings.Name;

            return Outcomes("Done");
        }
    }
}