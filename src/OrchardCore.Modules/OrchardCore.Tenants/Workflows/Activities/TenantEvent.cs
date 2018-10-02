using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities
{
    public abstract class TenantEvent : TenantActivity, IEvent
    {
        protected TenantEvent(IShellSettingsManager shellSettingsManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer localizer) 
            : base(shellSettingsManager, scriptEvaluator, localizer)
        {
        }

        public IList<string> ContentTypeFilter
        {
            get => GetProperty<IList<string>>(defaultValue: () => new List<string>());
            set => SetProperty(value);
        }

        public override async Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var content = await GetTenantAsync(workflowContext);

            if (content == null)
            {
                return false;
            }

            //var contentTypes = ContentTypeFilter.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            // "" means 'any'.
            return true; //!contentTypes.Any() || contentTypes.Any(contentType => content.ContentItem.ContentType == contentType);
        }


        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Halt();
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Done");
        }
    }
}