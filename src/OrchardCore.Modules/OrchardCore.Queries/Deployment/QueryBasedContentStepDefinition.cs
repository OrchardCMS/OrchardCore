using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Queries.Deployment;

internal sealed class QueryBasedContentStepDefinition : IDeploymentStepDefinition
{
    private readonly IQueryManager _queries;

    public QueryBasedContentStepDefinition(IQueryManager queries)
    {
        _queries = queries;
    }

    public string Type => nameof(QueryBasedContentDeploymentStep);

    public JsonObject GetSchema() => new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["queryName"] = new JsonObject { ["type"] = "string", ["minLength"] = 1 },
            ["queryParameters"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
            ["exportAsSetupRecipe"] = new JsonObject { ["type"] = "boolean" },
        },
    };

    public JsonObject Describe(DeploymentStep step)
    {
        var query = GetStep(step);
        return new JsonObject
        {
            ["queryName"] = query.QueryName,
            ["queryParameters"] = query.QueryParameters,
            ["exportAsSetupRecipe"] = query.ExportAsSetupRecipe,
        };
    }

    public async ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values)
    {
        var query = GetStep(step);
        var errors = new Dictionary<string, string[]>();
        var properties = GetSchema()["properties"].AsObject();
        if (values is null || values.Any(value => !properties.ContainsKey(value.Key)))
        {
            errors["values"] = ["Provide only properties from the deployment step schema."];
            return errors;
        }

        var name = query.QueryName;
        var parameters = query.QueryParameters;
        var setup = query.ExportAsSetupRecipe;
        foreach (var (property, value) in values)
        {
            if (property == "exportAsSetupRecipe")
            {
                if (value is JsonValue scalar && scalar.TryGetValue<bool>(out var flag))
                {
                    setup = flag;
                }
                else
                {
                    errors[property] = ["Provide a Boolean value."];
                }
            }
            else if (property == "queryParameters" && value is null)
            {
                parameters = null;
            }
            else if (value is JsonValue scalar && scalar.TryGetValue<string>(out var text))
            {
                if (property == "queryName")
                {
                    name = text;
                }
                else
                {
                    parameters = text;
                }
            }
            else
            {
                errors[property] = ["Provide a string value."];
            }
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        foreach (var (property, messages) in await QueryDeploymentConfiguration.ValidateAsync(_queries, name, parameters))
        {
            errors[property == nameof(QueryBasedContentDeploymentStep.QueryName) ? "queryName" : "queryParameters"] = messages;
        }

        if (errors.Count == 0)
        {
            query.QueryName = name;
            query.QueryParameters = parameters;
            query.ExportAsSetupRecipe = setup;
        }

        return errors;
    }

    private static QueryBasedContentDeploymentStep GetStep(DeploymentStep step) => step is QueryBasedContentDeploymentStep query
        ? query : throw new ArgumentException("The step does not match this configuration contract.", nameof(step));
}
