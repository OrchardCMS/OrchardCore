using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.Cli;

internal sealed partial class CliApplication
{
    private Command CreateGraphQLCommand()
    {
        var command = new Command("graphql", "Execute GraphQL documents and inspect the tenant schema directly");
        var endpoint = new Option<string>("--endpoint")
        {
            Description = "GraphQL path relative to the selected tenant",
            DefaultValueFactory = _ => "api/graphql",
            Recursive = true,
        };
        command.Options.Add(endpoint);

        var execute = new Command("execute", "Execute a GraphQL query or mutation without OpenAPI discovery");
        var query = new Option<string?>("--query") { Description = "Inline GraphQL document (quote it to protect $variables from the shell)" };
        var file = new Option<FileInfo?>("--file") { Description = "Read a GraphQL document from a UTF-8 file" };
        var stdin = new Option<bool>("--stdin") { Description = "Read a GraphQL document from standard input" };
        var namedQuery = new Option<string?>("--named-query") { Description = "Execute an Orchard server-registered named query instead of a document" };
        var variables = new Option<string?>("--variables") { Description = "Variables as a JSON object" };
        var variablesFile = new Option<FileInfo?>("--variables-file") { Description = "Read the variables JSON object from a UTF-8 file" };
        var operationName = new Option<string?>("--operation-name") { Description = "Operation to execute when the document contains multiple operations" };
        foreach (var option in new Option[] { query, file, stdin, namedQuery, variables, variablesFile, operationName })
        {
            execute.Options.Add(option);
        }
        execute.SetAction(async (parseResult, cancellationToken) =>
        {
            var request = await GraphQLRequestBuilder.CreateAsync(parseResult.GetValue(query), parseResult.GetValue(file), parseResult.GetValue(stdin),
                parseResult.GetValue(namedQuery), parseResult.GetValue(variables), parseResult.GetValue(variablesFile), parseResult.GetValue(operationName), Console.In, cancellationToken);
            return await ExecuteGraphQLAsync(parseResult, parseResult.GetValue(endpoint)!, request, cancellationToken);
        });

        var schema = new Command("schema", "Read GraphQL introspection as JSON, optionally for one type");
        var type = new Option<string?>("--type") { Description = "Inspect a single GraphQL type by its exact name" };
        schema.Options.Add(type);
        schema.SetAction((parseResult, cancellationToken) => ExecuteGraphQLAsync(parseResult, parseResult.GetValue(endpoint)!,
            GraphQLRequestBuilder.Introspection(parseResult.GetValue(type)), cancellationToken));
        command.Subcommands.Add(execute);
        command.Subcommands.Add(schema);
        return command;
    }

    private async Task<int> ExecuteGraphQLAsync(ParseResult parseResult, string endpoint, JsonObject payload, CancellationToken cancellationToken)
    {
        var context = RequireContext(parseResult.GetValue(_contextOption));
        // Validate the destination before reading credentials or contacting the authority.
        var uri = BuildRequestUri(new Uri(context.TenantUrl, UriKind.Absolute), endpoint, new Dictionary<string, string>());
        var accessToken = await ResolveAccessTokenAsync(context, null, null, false, cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json"),
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/graphql-response+json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        AddAuthorization(request, accessToken);

        // Do not retry: a mutation may already have committed when a connection fails.
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        JsonDocument? document = null;
        try
        {
            document = JsonDocument.Parse(content);
        }
        catch (JsonException) when (!response.IsSuccessStatusCode)
        {
            // HTML login/error pages are HTTP failures, not GraphQL results.
        }

        using (document)
        {
            var result = document?.RootElement ?? default;
            var hasErrors = result.ValueKind == JsonValueKind.Object && result.TryGetProperty("errors", out var errors)
                && errors.ValueKind == JsonValueKind.Array && errors.GetArrayLength() > 0;
            var hasData = result.ValueKind == JsonValueKind.Object && result.TryGetProperty("data", out _);
            if (hasErrors || (response.IsSuccessStatusCode && hasData))
            {
                if (hasErrors && CliUtilities.ParseOutputFormat(parseResult.GetValue(_outputOption)) == OutputFormat.Human)
                {
                    await Console.Error.WriteLineAsync(HumanErrorFormatter.Format("GraphQL returned errors.", JsonNode.Parse(result.GetRawText()), GetCorrelationId(response)));
                    if (result.TryGetProperty("data", out var data) && data.ValueKind != JsonValueKind.Null)
                    {
                        await WriteOutputAsync(parseResult, data, cancellationToken);
                    }
                    await Console.Error.WriteLineAsync("The response may contain partial data; check it before retrying a mutation.");
                    return 4;
                }

                // Preserve the GraphQL envelope, including partial data, even for HTTP 400/401.
                // HTTP POST alone does not make a GraphQL query a management mutation.
                await WriteOutputAsync(parseResult, result, cancellationToken);
                if (hasErrors)
                {
                    await Console.Error.WriteLineAsync("GraphQL returned errors. The response may contain partial data; check it before retrying a mutation.");
                    return 4;
                }
                return 0;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException((int)response.StatusCode,
                    $"GraphQL request POST {uri.AbsolutePath} failed with {(int)response.StatusCode} {response.ReasonPhrase}.",
                    document is null ? null : JsonNode.Parse(result.GetRawText()), GetCorrelationId(response));
            }

            throw new CliException("The endpoint did not return a GraphQL response containing data or errors. Check the selected tenant and --endpoint.");
        }
    }
}
