using System.Text.Json.Nodes;

namespace OrchardCore.Cli;

internal static class GraphQLRequestBuilder
{
    public static async Task<JsonObject> CreateAsync(string? query, FileInfo? file, bool stdin, string? namedQuery,
        string? variables, FileInfo? variablesFile, string? operationName, TextReader input, CancellationToken cancellationToken)
    {
        if ((query is not null ? 1 : 0) + (file is not null ? 1 : 0) + (stdin ? 1 : 0) + (namedQuery is not null ? 1 : 0) != 1)
        {
            throw new CliException("Provide exactly one of --query, --file, --stdin, or --named-query.");
        }
        if (variables is not null && variablesFile is not null)
        {
            throw new CliException("Use either --variables or --variables-file.");
        }

        var document = file is not null ? await ReadFileAsync(file, cancellationToken)
            : stdin ? await input.ReadToEndAsync(cancellationToken) : query;
        if (string.IsNullOrWhiteSpace(namedQuery ?? document))
        {
            throw new CliException("The GraphQL document or named query must not be empty.");
        }
        if (operationName is not null && string.IsNullOrWhiteSpace(operationName))
        {
            throw new CliException("The operation name must not be empty.");
        }

        var request = new JsonObject();
        request[namedQuery is null ? "query" : "namedQuery"] = namedQuery ?? document;
        if (operationName is not null)
        {
            request["operationName"] = operationName;
        }
        var variablesJson = variablesFile is not null ? await ReadFileAsync(variablesFile, cancellationToken) : variables;
        if (variablesJson is not null)
        {
            if (JsonNode.Parse(variablesJson) is not JsonObject values)
            {
                throw new CliException("GraphQL variables must be a JSON object.");
            }
            request["variables"] = values;
        }
        return request;
    }

    public static JsonObject Introspection(string? typeName)
    {
        if (typeName is not null && string.IsNullOrWhiteSpace(typeName))
        {
            throw new CliException("The GraphQL type name must not be empty.");
        }
        var query = typeName is null
            ? """
              query OcIntrospection {
                __schema {
                  queryType { name }
                  mutationType { name }
                  subscriptionType { name }
                  types { ...FullType }
                  directives { name description locations args { ...InputValue } }
                }
              }
              """
            : "query OcIntrospection($name: String!) { __type(name: $name) { ...FullType } }";
        // Follow nested list/non-null wrappers as the standard introspection query does.
        var typeReference = "kind name";
        for (var depth = 0; depth < 8; depth++)
        {
            typeReference = "kind name ofType { " + typeReference + " }";
        }
        query += """

            fragment FullType on __Type {
              kind name description
              fields(includeDeprecated: true) {
                name description args { ...InputValue }
                type { ...TypeRef } isDeprecated deprecationReason
              }
              inputFields { ...InputValue }
              interfaces { ...TypeRef }
              enumValues(includeDeprecated: true) { name description isDeprecated deprecationReason }
              possibleTypes { ...TypeRef }
            }
            fragment InputValue on __InputValue {
              name description type { ...TypeRef } defaultValue
            }
            """;
        query += "\nfragment TypeRef on __Type { " + typeReference + " }";
        var request = new JsonObject { ["query"] = query, ["operationName"] = "OcIntrospection" };
        if (typeName is not null)
        {
            request["variables"] = new JsonObject { ["name"] = typeName };
        }
        return request;
    }

    private static async Task<string> ReadFileAsync(FileInfo file, CancellationToken cancellationToken)
    {
        try
        {
            return await File.ReadAllTextAsync(file.FullName, cancellationToken);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new CliException($"Cannot read GraphQL input file '{file.FullName}': {exception.Message}");
        }
    }
}
