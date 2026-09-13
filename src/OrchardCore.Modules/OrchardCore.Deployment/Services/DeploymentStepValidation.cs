using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Deployment.Deployment;
using OrchardCore.Deployment.Steps;

namespace OrchardCore.Deployment.Services;

internal static class DeploymentStepValidation
{
    internal static Dictionary<string, string[]> Validate(DeploymentStep step)
    {
        var errors = new Dictionary<string, string[]>();
        if (step is JsonRecipeDeploymentStep recipe)
        {
            try
            {
                if (JsonNode.Parse(recipe.Json ?? "") is not JsonObject value
                    || value["name"] is not JsonValue name || !name.TryGetValue<string>(out var text)
                    || string.IsNullOrWhiteSpace(text))
                {
                    errors["json"] = ["The recipe must be an object with a nonempty name string."];
                }
            }
            catch (Exception exception) when (exception is JsonException or ArgumentException)
            {
                errors["json"] = ["Invalid JSON supplied."];
            }
        }
        else if (step is CustomFileDeploymentStep file)
        {
            var path = file.FileName;
            if (string.IsNullOrWhiteSpace(path) || path.StartsWith('/') || path.Contains('\\') || path.Contains(':')
                || path.Split('/').Any(segment => segment is "" or "." or "..") || path.Any(char.IsControl)
                || string.Equals(path, "Recipe.json", StringComparison.OrdinalIgnoreCase))
            {
                errors["fileName"] = ["Use a relative package file path without traversal; Recipe.json is reserved."];
            }
        }
        return errors;
    }

    internal static void Normalize(DeploymentStep step)
    {
        if (step is DeploymentPlanDeploymentStep plans && plans.IncludeAll)
        {
            plans.DeploymentPlanNames = [];
        }
        if (step is CustomFileDeploymentStep file)
        {
            file.FileContent ??= string.Empty;
        }
    }
}
