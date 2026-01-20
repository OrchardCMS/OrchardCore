using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.ViewModels
{
    public class SelectWorkflowTypeViewModel
    {
        public string HtmlName { get; set; }
        public IList<WorkflowTypeSelection> WorkflowTypeSelections { get; set; }
    }

    public class WorkflowTypeSelection
    {
        public bool IsSelected { get; set; }
        public WorkflowType WorkflowType { get; set; }

        public static async Task<IList<WorkflowTypeSelection>> BuildAsync(IWorkflowTypeStore workflowTypeStore, string selectedWorkflowTypeId)
        {
            var workflowTypes = await workflowTypeStore.ListAsync();
            var selections = workflowTypes
                .Select(x => new WorkflowTypeSelection
                {
                    IsSelected = x.WorkflowTypeId == selectedWorkflowTypeId,
                    WorkflowType = x
                })
                .OrderBy(x => x.WorkflowType.Name)
                .ToList();

            return selections;
        }
    }
}
