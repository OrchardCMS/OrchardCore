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

    protected override async Task ProcessAsync(ContentDefinitionDeploymentStep step, DeploymentPlanResult result)
    {
        var contentTypeDefinitionRecord = await _contentDefinitionStore.LoadContentDefinitionAsync();

        var contentTypes = step.IncludeAll
            ? contentTypeDefinitionRecord.ContentTypeDefinitionRecords
            : contentTypeDefinitionRecord.ContentTypeDefinitionRecords
                .Where(x => step.ContentTypes.Contains(x.Name));

        var contentParts = step.IncludeAll
            ? contentTypeDefinitionRecord.ContentPartDefinitionRecords
            : contentTypeDefinitionRecord.ContentPartDefinitionRecords
                    .Where(x => step.ContentParts.Contains(x.Name));

        result.Steps.Add(new JsonObject
        {
            ["name"] = "ContentDefinition",
            ["ContentTypes"] = JArray.FromObject(contentTypes),
            ["ContentParts"] = JArray.FromObject(contentParts),
        });
    }
}
