using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.Environment.Shell;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities
{
    public abstract class TenantActivity : Activity
    {
        protected TenantActivity(IShellSettingsManager shellSettingsManager, IShellHost shellHost, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer localizer)
        {
            ShellSettingsManager = shellSettingsManager;
            ShellHost = shellHost;
            ScriptEvaluator = scriptEvaluator;
            T = localizer;
        }

        public WorkflowExpression<string> TenantName
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        protected IShellSettingsManager ShellSettingsManager { get; }
        protected IShellHost ShellHost { get; }
        protected IWorkflowScriptEvaluator ScriptEvaluator { get; }
        protected IStringLocalizer T { get; }
        public override LocalizedString Category => T["Tenant"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Done");
        }

        //protected virtual async Task<ShellSettings> GetTenantAsync(WorkflowExecutionContext workflowContext)
        //{
        //    // Try and evaluate a content item from the Content expression, if provided.
        //    //if (!string.IsNullOrWhiteSpace(Tenant.Expression))
        //    //{
        //        //var expression = new WorkflowExpression<object> { Expression = Tenant.Expression };
        //        //var tenantJson = JsonConvert.SerializeObject(await ScriptEvaluator.EvaluateAsync(expression, workflowContext));
        //        //var res = JsonConvert.DeserializeObject<ShellSettings>(tenantJson);
        //        //return res;
        //    //}

        //    //// If no expression was provided, see if the content item was provided as an input or as a property.
        //    //var content = workflowContext.Input.GetValue<IContent>(TenantsHandler.ContentItemInputKey)
        //    //    ?? workflowContext.Properties.GetValue<IContent>(TenantsHandler.ContentItemInputKey);

        //    //if (content != null)
        //    //{
        //    //    return content;
        //    //}

        //    return null;
        //}
    }
}