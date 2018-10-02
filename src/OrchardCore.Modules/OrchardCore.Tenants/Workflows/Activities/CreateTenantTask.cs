using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities
{
    public class CreateTenantTask : TenantTask
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public CreateTenantTask(IShellSettingsManager shellSettingsManager, IWorkflowExpressionEvaluator expressionEvaluator, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<CreateTenantTask> localizer)
            : base(shellSettingsManager, scriptEvaluator, localizer)
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
        
        public WorkflowExpression<string> TenantProperties
        {
            get => GetProperty(() => new WorkflowExpression<string>(JsonConvert.SerializeObject(new { TitlePart = new { Title = "" } }, Formatting.Indented)));
            set => SetProperty(value);
        }

        public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return !string.IsNullOrEmpty(ContentType);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public async override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var shellSettings = new ShellSettings();

            if (!string.IsNullOrWhiteSpace(TenantProperties.Expression))
            {
                var tenantProperties = await _expressionEvaluator.EvaluateAsync(TenantProperties, workflowContext);
                var propertyObject = JObject.Parse(tenantProperties);

                // shellSettings = new ShellSettings();

                //var shellSettings = new ShellSettings
                //{
                //    Name = model.Name,
                //    RequestUrlPrefix = model.RequestUrlPrefix?.Trim(),
                //    RequestUrlHost = model.RequestUrlHost,
                //    ConnectionString = model.ConnectionString,
                //    TablePrefix = model.TablePrefix,
                //    DatabaseProvider = model.DatabaseProvider,
                //    State = TenantState.Uninitialized,
                //    Secret = Guid.NewGuid().ToString(),
                //    RecipeName = model.RecipeName
                //};
            }
            
            ShellSettingsManager.SaveSettings(shellSettings);
            //var shellContext = await _orchardHost.GetOrCreateShellContextAsync(shellSettings);

            workflowContext.LastResult = shellSettings;
            workflowContext.CorrelationId = shellSettings.Name;
            //workflowContext.Properties[TenantsHandler.ContentItemInputKey] = contentItem;

            return Outcomes("Done");
        }
    }
}