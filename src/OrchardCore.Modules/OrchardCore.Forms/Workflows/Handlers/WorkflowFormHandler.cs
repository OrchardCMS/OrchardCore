using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Forms.Workflows.Handlers
{
    public class WorkflowFormHandler : FormHanderBase
    {
        private readonly IWorkflowTypeStore _workflowTypeStore;
        private readonly IWorkflowManager _workflowManager;

        public WorkflowFormHandler(IWorkflowTypeStore workflowTypeStore, IWorkflowManager workflowManager)
        {
            _workflowTypeStore = workflowTypeStore;
            _workflowManager = workflowManager;
        }

        protected override async Task OnSubmittedAsync(FormSubmittedContext context)
        {
            var contentItem = await context.FormContentItem.Value;
            var formWorkflowPart = contentItem.As<FormWorkflowPart>();
            var workflowTypeId = formWorkflowPart.WorkflowTypeId;
            var workflowType = workflowTypeId != null ? _workflowTypeStore.GetAsync(workflowTypeId) : default;

            if (workflowType == null)
            {
                return;
            }

            var input = new Dictionary<string, object>
            {
                { "", }
            };
            _workflowManager.StartWorkflowAsync(workflowType, );
        }
    }
}