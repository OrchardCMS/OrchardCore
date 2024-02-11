using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Controllers;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Recipes
{
    public class WorkflowTypeStep : IRecipeStepHandler
    {
        private readonly IWorkflowTypeStore _workflowTypeStore;
        private readonly ISecurityTokenService _securityTokenService;
        private readonly IUrlHelper _urlHelper;

        public WorkflowTypeStep(IWorkflowTypeStore workflowTypeStore,
            ISecurityTokenService securityTokenService,
            IActionContextAccessor actionContextAccessor,
            IUrlHelperFactory urlHelperFactory)
        {
            _workflowTypeStore = workflowTypeStore;
            _securityTokenService = securityTokenService;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "WorkflowType", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<WorkflowStepModel>();

            foreach (var token in model.Data.Cast<JsonObject>())
            {
                var workflow = token.ToObject<WorkflowType>();

                foreach (var activity in workflow.Activities.Where(a => a.Name == nameof(HttpRequestEvent)))
                {
                    var tokenLifeSpan = activity.Properties["TokenLifeSpan"];
                    if (tokenLifeSpan != null)
                    {
                        activity.Properties["Url"] = ReGenerateHttpRequestEventUrl(workflow, activity, tokenLifeSpan.ToObject<int>());
                    }
                }

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

        private string ReGenerateHttpRequestEventUrl(WorkflowType workflow, ActivityRecord activity, int tokenLifeSpan)
        {
            var token = _securityTokenService.CreateToken(new WorkflowPayload(workflow.WorkflowTypeId, activity.ActivityId),
                TimeSpan.FromDays(tokenLifeSpan == 0 ? HttpWorkflowController.NoExpiryTokenLifespan : tokenLifeSpan));

            return _urlHelper.Action("Invoke", "HttpWorkflow", new { token });
        }
    }

    public class WorkflowStepModel
    {
        public JsonArray Data { get; set; }
    }
}
