using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Extensions;
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
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public WorkflowTypeStep(IWorkflowTypeStore workflowTypeStore,
        ISecurityTokenService securityTokenService,
        IHttpContextAccessor httpContextAccessor,
        IUrlHelperFactory urlHelperFactory,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
        : base("WorkflowType")
    {
        _workflowTypeStore = workflowTypeStore;
        _securityTokenService = securityTokenService;
        _httpContextAccessor = httpContextAccessor;
        _urlHelperFactory = urlHelperFactory;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<WorkflowStepModel>();
        var urlHelper = await GetUrlHelperAsync();

        foreach (var token in model.Data.Cast<JsonObject>())
        {
            var workflow = token.ToObject<WorkflowType>(_jsonSerializerOptions);

            var existing = await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId);

            if (existing is null)
            {
                workflow.Id = 0;

                if (urlHelper is not null)
                {
                    foreach (var activity in workflow.Activities.Where(a => a.Name == nameof(HttpRequestEvent)))
                    {
                        if (!activity.Properties.TryGetPropertyValue("TokenLifeSpan", out var tokenLifeSpan))
                        {
                            continue;
                        }

                        activity.Properties["Url"] = ReGenerateHttpRequestEventUrl(urlHelper, workflow, activity, tokenLifeSpan.ToObject<int>());
                    }
                }
            }
            else
            {
                await _workflowTypeStore.DeleteAsync(existing);
            }

            await _workflowTypeStore.SaveAsync(workflow);
        }
    }

    private async Task<IUrlHelper> GetUrlHelperAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        var actionContext = await httpContext.GetActionContextAsync();
        return _urlHelperFactory.GetUrlHelper(actionContext);
    }

    private string ReGenerateHttpRequestEventUrl(IUrlHelper urlHelper, WorkflowType workflow, ActivityRecord activity, int tokenLifeSpan)
    {
        var token = _securityTokenService.CreateToken(new WorkflowPayload(workflow.WorkflowTypeId, activity.ActivityId),
            TimeSpan.FromDays(tokenLifeSpan == 0 ? HttpWorkflowController.NoExpiryTokenLifespan : tokenLifeSpan));

        return urlHelper.Action("Invoke", "HttpWorkflow", new { token });
    }
}

public sealed class WorkflowStepModel
{
    public JsonArray Data { get; set; }
}
