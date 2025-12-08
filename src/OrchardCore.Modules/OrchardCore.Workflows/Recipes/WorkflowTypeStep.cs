using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Controllers;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Recipes;

public sealed class WorkflowTypeStep : NamedRecipeStepHandler
{
    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly ISecurityTokenService _securityTokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public WorkflowTypeStep(IWorkflowTypeStore workflowTypeStore,
        ISecurityTokenService securityTokenService,
        IHttpContextAccessor httpContextAccessor,
        LinkGenerator linkGenerator,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
        : base("WorkflowType")
    {
        _workflowTypeStore = workflowTypeStore;
        _securityTokenService = securityTokenService;
        _httpContextAccessor = httpContextAccessor;
        _linkGenerator = linkGenerator;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<WorkflowStepModel>();

        foreach (var token in model.Data.Cast<JsonObject>())
        {
            var workflow = token.ToObject<WorkflowType>(_jsonSerializerOptions);

            var existing = await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId);

            if (existing is null)
            {
                workflow.Id = 0;

                foreach (var activity in workflow.Activities.Where(a => a.Name == nameof(HttpRequestEvent)))
                {
                    if (!activity.Properties.TryGetPropertyValue("TokenLifeSpan", out var tokenLifeSpan))
                    {
                        continue;
                    }

                    activity.Properties["Url"] = GetRegenerateHttpRequestEventUrl(workflow, activity, tokenLifeSpan.ToObject<int>());
                }
            }
            else
            {
                await _workflowTypeStore.DeleteAsync(existing);
            }

            await _workflowTypeStore.SaveAsync(workflow);
        }
    }

    private string GetRegenerateHttpRequestEventUrl(WorkflowType workflow, ActivityRecord activity, int tokenLifeSpan)
    {
        var lifespan = TimeSpan.FromDays(tokenLifeSpan == 0 ? HttpWorkflowController.NoExpiryTokenLifespan : tokenLifeSpan);

        var token = _securityTokenService.CreateToken(new WorkflowPayload(workflow.WorkflowTypeId, activity.ActivityId), lifespan);

        return _linkGenerator.GetPathByAction(_httpContextAccessor.HttpContext, "Invoke", "HttpWorkflow", new { area = "OrchardCore.Workflows", token });
    }
}

public sealed class WorkflowStepModel
{
    public JsonArray Data { get; set; }
}
