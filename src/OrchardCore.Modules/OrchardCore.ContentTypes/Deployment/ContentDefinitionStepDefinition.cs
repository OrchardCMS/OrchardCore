using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment;

internal sealed class ContentDefinitionStepDefinition : IDeploymentStepDefinition
{
    private readonly IContentDefinitionManager _definitions;

    public ContentDefinitionStepDefinition(string type, IContentDefinitionManager definitions)
    {
        Type = type;
        _definitions = definitions;
    }

    public string Type { get; }
    private bool Deletes => Type == nameof(DeleteContentDefinitionDeploymentStep);

    public JsonObject GetSchema()
    {
        var properties = new JsonObject
        {
            ["contentTypes"] = new JsonObject { ["type"] = "array", ["items"] = new JsonObject { ["type"] = "string", ["minLength"] = 1 } },
            ["contentParts"] = new JsonObject { ["type"] = "array", ["items"] = new JsonObject { ["type"] = "string", ["minLength"] = 1 } },
        };
        if (!Deletes)
        {
            properties["includeAll"] = new JsonObject { ["type"] = "boolean" };
        }

        return new JsonObject { ["type"] = "object", ["additionalProperties"] = false, ["properties"] = properties };
    }

    public JsonObject Describe(DeploymentStep step)
    {
        var (all, types, parts) = Read(step);
        var result = new JsonObject { ["contentTypes"] = new JsonArray((types ?? []).Select(value => (JsonNode)JsonValue.Create(value)).ToArray()),
            ["contentParts"] = new JsonArray((parts ?? []).Select(value => (JsonNode)JsonValue.Create(value)).ToArray()), };
        if (!Deletes)
        {
            result["includeAll"] = all;
        }

        return result;
    }

    public async ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values)
    {
        var (all, types, parts) = Read(step);
        var errors = new Dictionary<string, string[]>();
        var properties = GetSchema()["properties"].AsObject();
        if (values is null || values.Any(value => !properties.ContainsKey(value.Key)))
        {
            errors["values"] = ["Provide only properties from the deployment step schema."];
            return errors;
        }
        foreach (var (name, value) in values)
        {
            if (name == "includeAll")
            {
                if (value is JsonValue scalar && scalar.TryGetValue<bool>(out var flag))
                {
                    all = flag;
                }
                else
                {
                    errors[name] = ["Provide a Boolean value."];
                }
            }
            else if (value is JsonArray array && array.All(item => item is JsonValue scalar
                && scalar.TryGetValue<string>(out var text) && !string.IsNullOrWhiteSpace(text)))
            {
                var names = array.Select(item => item.GetValue<string>()).ToArray();
                if (name == "contentTypes")
                {
                    types = names;
                }
                else
                {
                    parts = names;
                }
            }
            else
            {
                errors[name] = ["Provide an array of non-empty definition names."];
            }
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        if (!Deletes)
        {
            (types, parts) = ContentDefinitionSelection.Normalize(all, types, parts);
        }
        if (!Deletes && !all)
        {
            foreach (var name in types)
            {
                if (await _definitions.GetTypeDefinitionAsync(name) is null)
                {
                    errors["contentTypes"] = ["Select existing content types."];
                }
            }
            foreach (var name in parts)
            {
                if (await _definitions.GetPartDefinitionAsync(name) is null)
                {
                    errors["contentParts"] = ["Select existing content parts."];
                }
            }
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        switch (step)
        {
            case ContentDefinitionDeploymentStep content:
                content.IncludeAll = all;
                content.ContentTypes = types;
                content.ContentParts = parts;
                break;
            case ReplaceContentDefinitionDeploymentStep replace:
                replace.IncludeAll = all;
                replace.ContentTypes = types;
                replace.ContentParts = parts;
                break;
            case DeleteContentDefinitionDeploymentStep delete:
                delete.ContentTypes = types;
                delete.ContentParts = parts;
                break;
        }
        return errors;
    }

    private (bool All, string[] Types, string[] Parts) Read(DeploymentStep step) => step switch
    {
        ContentDefinitionDeploymentStep content when Type == nameof(ContentDefinitionDeploymentStep) => (content.IncludeAll, content.ContentTypes, content.ContentParts),
        ReplaceContentDefinitionDeploymentStep replace when Type == nameof(ReplaceContentDefinitionDeploymentStep) => (replace.IncludeAll, replace.ContentTypes, replace.ContentParts),
        DeleteContentDefinitionDeploymentStep delete when Deletes => (false, delete.ContentTypes, delete.ContentParts),
        _ => throw new ArgumentException("The step does not match this configuration contract.", nameof(step)),
    };
}
