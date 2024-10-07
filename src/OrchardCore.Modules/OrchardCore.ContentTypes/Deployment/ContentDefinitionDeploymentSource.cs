using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment;

public class ContentDefinitionDeploymentSource
    : DeploymentSourceBase<ContentDefinitionDeploymentStep>
{
    private readonly IContentDefinitionStore _contentDefinitionStore;

    public ContentDefinitionDeploymentSource(IContentDefinitionStore contentDefinitionStore)
    {
        _contentDefinitionStore = contentDefinitionStore;
    }

    public override async Task ProcessDeploymentStepAsync(DeploymentPlanResult result)
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
            ["name"] = "ContentDefinition",
            ["ContentTypes"] = JArray.FromObject(contentTypes),
            ["ContentParts"] = JArray.FromObject(contentParts),
        });
    }
}
