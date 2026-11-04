using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan;

internal sealed class ContentItemDeploymentStepDefinition : IDeploymentStepDefinition
{
    private readonly IContentManager _contentManager;
    public ContentItemDeploymentStepDefinition(IContentManager contentManager) => _contentManager = contentManager;
    public string Type => nameof(ContentItemDeploymentStep);
    public JsonObject GetSchema() => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema", ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject { ["contentItemId"] = new JsonObject { ["type"] = "string", ["description"] = "An existing content item in this tenant." } },
    };
    public JsonObject Describe(DeploymentStep step) => step is ContentItemDeploymentStep item
        ? new() { ["contentItemId"] = item.ContentItemId } : throw new ArgumentException("The step is not a content item selector.", nameof(step));
    public async ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values)
    {
        var errors = new Dictionary<string, string[]>();
        if (step is not ContentItemDeploymentStep item || values is null)
        {
            errors["values"] = ["A content item selector configuration is required."];
            return errors;
        }
        foreach (var (key, value) in values)
        {
            if (key != "contentItemId" || value is not JsonValue text || !text.TryGetValue<string>(out _))
            {
                errors[key] = ["Provide a string contentItemId; other properties are not supported."];
            }
        }
        if (errors.Count > 0) { return errors; }
        var id = values.TryGetPropertyValue("contentItemId", out var node) ? node.GetValue<string>() : item.ContentItemId;
        if (!await ContentItemDeploymentStepEditor.TryApplyAsync(_contentManager, item, id))
        {
            errors["contentItemId"] = ["Your content item does not exist."];
        }
        return errors;
    }
}
