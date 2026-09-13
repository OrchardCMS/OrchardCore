using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

internal sealed class ExportContentSelectionDefinition : IDeploymentStepDefinition
{
    private readonly IContentManager _contentManager;
    public ExportContentSelectionDefinition(IContentManager contentManager) => _contentManager = contentManager;
    public string Type => nameof(ExportContentToDeploymentTargetDeploymentStep);
    public JsonObject GetSchema() => new()
    {
        ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["contentItemIds"] = new JsonObject { ["type"] = "array", ["minItems"] = 1, ["maxItems"] = 200, ["items"] = new JsonObject { ["type"] = "string", ["minLength"] = 1 } },
            ["latest"] = new JsonObject { ["type"] = "boolean", ["description"] = "Export latest versions instead of published versions." },
        },
    };
    public JsonObject Describe(DeploymentStep step)
    {
        var value = Get(step);
        return new() { ["contentItemIds"] = value.ContentItemIds is null ? null : new JsonArray(value.ContentItemIds.Select(id => (JsonNode)JsonValue.Create(id)).ToArray()), ["latest"] = value.Latest };
    }
    public async ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values)
    {
        var value = Get(step);
        var errors = new Dictionary<string, string[]>();
        if (values is null || values.Any(property => property.Key is not ("contentItemIds" or "latest")))
        {
            errors["values"] = ["Provide only contentItemIds and latest."];
            return errors;
        }
        var ids = value.ContentItemIds;
        var latest = value.Latest;
        if (values.TryGetPropertyValue("contentItemIds", out var idsNode))
        {
            if (idsNode is JsonArray array && array.Count is > 0 and <= 200 && array.All(node => node is JsonValue text && text.TryGetValue<string>(out var id) && !string.IsNullOrWhiteSpace(id) && id.Length <= 128))
            {
                ids = array.Select(node => node.GetValue<string>()).Distinct(StringComparer.Ordinal).ToArray();
            }
            else { errors["contentItemIds"] = ["Provide between 1 and 200 content item IDs."]; }
        }
        if (values.TryGetPropertyValue("latest", out var latestNode))
        {
            if (latestNode is JsonValue flag && flag.TryGetValue<bool>(out var requested)) { latest = requested; }
            else { errors["latest"] = ["Provide a Boolean value."]; }
        }
        if (ids is null || ids.Length == 0) { errors["contentItemIds"] = ["Explicit content item IDs are required for remote configuration."]; }
        if (errors.Count > 0) { return errors; }
        foreach (var id in ids)
        {
            if (await _contentManager.GetAsync(id, latest ? VersionOptions.Latest : VersionOptions.Published) is null)
            {
                errors["contentItemIds"] = ["Every selected content version must exist in this tenant."];
                return errors;
            }
        }
        value.ContentItemIds = ids;
        value.Latest = latest;
        return errors;
    }
    private static ExportContentToDeploymentTargetDeploymentStep Get(DeploymentStep step) => step as ExportContentToDeploymentTargetDeploymentStep
        ?? throw new ArgumentException("The step is not a content export selection.", nameof(step));
}
