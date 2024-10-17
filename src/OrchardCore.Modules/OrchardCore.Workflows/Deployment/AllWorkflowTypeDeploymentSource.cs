using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Json;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Deployment;

public class AllWorkflowTypeDeploymentSource
    : DeploymentSourceBase<AllWorkflowTypeDeploymentStep>
{
    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AllWorkflowTypeDeploymentSource(
        IWorkflowTypeStore workflowTypeStore,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
    {
        _workflowTypeStore = workflowTypeStore;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
    }

    protected override async Task ProcessAsync(AllWorkflowTypeDeploymentStep step, DeploymentPlanResult result)
        => ProcessWorkflowType(result, await _workflowTypeStore.ListAsync(), _jsonSerializerOptions);

    public static void ProcessWorkflowType(DeploymentPlanResult result, IEnumerable<WorkflowType> workflowTypes, JsonSerializerOptions jsonSerializerOptions)
    {
        var data = new JsonArray();

        foreach (var workflowType in workflowTypes)
        {
            var objectData = JObject.FromObject(workflowType, jsonSerializerOptions);

            // Don't serialize the Id as it could be interpreted as an updated object when added back to YesSql
            objectData.Remove(nameof(workflowType.Id));
            data.Add(objectData);
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = "WorkflowType",
            ["data"] = data,
        });
    }
}
