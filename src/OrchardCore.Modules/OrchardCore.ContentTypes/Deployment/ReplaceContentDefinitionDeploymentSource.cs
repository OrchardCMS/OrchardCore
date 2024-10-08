using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment;

public class ReplaceContentDefinitionDeploymentSource
    : DeploymentSourceBase<ReplaceContentDefinitionDeploymentStep>
{
    private readonly IContentDefinitionStore _contentDefinitionStore;

    public ReplaceContentDefinitionDeploymentSource(IContentDefinitionStore contentDefinitionStore)
    {
        _contentDefinitionStore = contentDefinitionStore;
    }

    protected override async Task ProcessAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        var contentTypeDefinitionRecord = await _contentDefinitionStore.LoadContentDefinitionAsync();

        var contentTypes = DeploymentStep.IncludeAll
            ? contentTypeDefinitionRecord.ContentTypeDefinitionRecords
            : contentTypeDefinitionRecord.ContentTypeDefinitionRecords
                .Where(x => DeploymentStep.ContentTypes.Contains(x.Name));

        var contentParts = DeploymentStep.IncludeAll
            ? contentTypeDefinitionRecord.ContentPartDefinitionRecords
            : contentTypeDefinitionRecord.ContentPartDefinitionRecords
                    .Where(x => DeploymentStep.ContentParts.Contains(x.Name));

        result.Steps.Add(new JsonObject
        {
            ["name"] = "ReplaceContentDefinition",
            ["ContentTypes"] = JArray.FromObject(contentTypes),
            ["ContentParts"] = JArray.FromObject(contentParts),
        });
    }
}
