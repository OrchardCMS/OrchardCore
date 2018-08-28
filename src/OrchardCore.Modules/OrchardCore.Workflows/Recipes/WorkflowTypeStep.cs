using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;

namespace OrchardCore.Workflows.Recipes
{
    public class WorkflowTypeStep : IRecipeStepHandler
    {
        private readonly ISession _session;
        private readonly IWorkflowTypeStore _workflowTypeStore;

        public WorkflowTypeStep(IWorkflowTypeStore workflowTypeStore, ISession session)
        {
            _workflowTypeStore = workflowTypeStore;
            _session = session;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "WorkflowType", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<WorkflowStepModel>();

            foreach (JObject token in model.Data)
            {
                var workflow = token.ToObject<WorkflowType>();

                var existing = await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId);

                if (existing == null)
                {
                    workflow.Id = 0;
                    await _workflowTypeStore.SaveAsync(workflow);
                }
            }

            return;
        }
    }

    public class WorkflowStepModel
    {
        public JArray Data { get; set; }
    }
}
