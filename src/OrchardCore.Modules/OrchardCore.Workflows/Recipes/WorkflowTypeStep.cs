using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Recipes
{
    public class WorkflowTypeStep : IRecipeStepHandler
    {
        private readonly IWorkflowTypeStore _workflowTypeStore;

        public WorkflowTypeStep(IWorkflowTypeStore workflowTypeStore)
        {
            _workflowTypeStore = workflowTypeStore;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "WorkflowType", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<WorkflowStepModel>();

            foreach (var token in model.Data.Cast<JObject>())
            {
                var workflow = token.ToObject<WorkflowType>();

                var existing = await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId);

                if (existing == null)
                {
                    workflow.Id = 0;
                }
                else
                {
                    await _workflowTypeStore.DeleteAsync(existing);
                }

                await _workflowTypeStore.SaveAsync(workflow);
            }
        }
    }

    public class WorkflowStepModel
    {
        public JArray Data { get; set; }
    }
}
