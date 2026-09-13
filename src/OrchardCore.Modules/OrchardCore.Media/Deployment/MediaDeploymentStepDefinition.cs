using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Media.Deployment;

internal sealed class MediaDeploymentStepDefinition : IDeploymentStepDefinition
{
    private readonly IMediaFileStore _files;

    public MediaDeploymentStepDefinition(IMediaFileStore files)
    {
        _files = files;
    }

    public string Type => nameof(MediaDeploymentStep);

    public JsonObject GetSchema() => new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["includeAll"] = new JsonObject { ["type"] = "boolean" },
            ["filePaths"] = new JsonObject { ["type"] = "array", ["items"] = new JsonObject { ["type"] = "string", ["minLength"] = 1 } },
            ["directoryPaths"] = new JsonObject { ["type"] = "array", ["items"] = new JsonObject { ["type"] = "string", ["minLength"] = 1 } },
        },
    };

    public JsonObject Describe(DeploymentStep step)
    {
        var media = GetStep(step);
        return new JsonObject
        {
            ["includeAll"] = media.IncludeAll,
            ["filePaths"] = new JsonArray((media.FilePaths ?? []).Select(path => (JsonNode)JsonValue.Create(path)).ToArray()),
            ["directoryPaths"] = new JsonArray((media.DirectoryPaths ?? []).Select(path => (JsonNode)JsonValue.Create(path)).ToArray()),
        };
    }

    public async ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values)
    {
        var media = GetStep(step);
        var errors = new Dictionary<string, string[]>();
        if (values is null || values.Any(property => !GetSchema()["properties"].AsObject().ContainsKey(property.Key)))
        {
            errors["values"] = ["Provide only properties from the deployment step schema."];
            return errors;
        }

        var includeAll = media.IncludeAll;
        var files = media.FilePaths;
        var directories = media.DirectoryPaths;
        foreach (var (name, value) in values)
        {
            if (name == "includeAll")
            {
                if (value is JsonValue scalar && scalar.TryGetValue<bool>(out var flag))
                {
                    includeAll = flag;
                }
                else
                {
                    errors[name] = ["Provide a Boolean value."];
                }
            }
            else if (value is JsonArray array && array.All(item => item is JsonValue scalar
                && scalar.TryGetValue<string>(out var path) && IsRelativePath(path)))
            {
                var paths = array.Select(item => item.GetValue<string>()).ToArray();
                if (name == "filePaths")
                {
                    files = paths;
                }
                else
                {
                    directories = paths;
                }
            }
            else
            {
                errors[name] = ["Provide an array of relative media paths without traversal segments."];
            }
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        (files, directories) = MediaDeploymentSelection.Normalize(includeAll, files, directories);
        foreach (var path in files)
        {
            if (await _files.GetFileInfoAsync(path) is null)
            {
                errors["filePaths"] = ["Select existing media files."];
            }
        }

        foreach (var path in directories)
        {
            if (await _files.GetDirectoryInfoAsync(path) is null)
            {
                errors["directoryPaths"] = ["Select existing media directories."];
            }
        }

        if (errors.Count == 0)
        {
            media.IncludeAll = includeAll;
            media.FilePaths = files;
            media.DirectoryPaths = directories;
        }

        return errors;
    }

    private static bool IsRelativePath(string path) => !string.IsNullOrWhiteSpace(path)
        && !path.Any(character => char.IsControl(character) || character is '\\' or ':')
        && path.Split('/').All(segment => segment.Length > 0 && segment is not "." and not "..");

    private static MediaDeploymentStep GetStep(DeploymentStep step) => step is MediaDeploymentStep media
        ? media : throw new ArgumentException("The step does not match this configuration contract.", nameof(step));
}
