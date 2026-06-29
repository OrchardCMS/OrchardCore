using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataOrchestrator.Models;
using OrchardCore.Queries;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Extracts records by executing a named Orchard Core <see cref="Query"/> (SQL, Lucene,
/// Elasticsearch, or any registered query source). The query results are streamed as records.
/// </summary>
public sealed class QuerySource : EtlSourceActivity
{
    public override string Name => nameof(QuerySource);

    public override string DisplayText => "Query";

    /// <summary>
    /// Gets or sets the technical name of the query to execute.
    /// </summary>
    public string QueryName
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets an optional JSON object of parameters passed to the query.
    /// </summary>
    public string ParametersJson
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public override IEnumerable<EtlOutcome> GetPossibleOutcomes()
    {
        return [new EtlOutcome("Done")];
    }

    public override Task<EtlActivityResult> ExecuteAsync(EtlExecutionContext context)
    {
        context.DataStream = ExtractAsync(
            context.ServiceProvider,
            QueryName,
            ParametersJson,
            context.CancellationToken);

        return Task.FromResult(Outcomes("Done"));
    }

    private static async IAsyncEnumerable<JsonObject> ExtractAsync(
        IServiceProvider serviceProvider,
        string queryName,
        string parametersJson,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(queryName))
        {
            yield break;
        }

        var queryManager = serviceProvider.GetService<IQueryManager>();

        if (queryManager == null)
        {
            yield break;
        }

        var query = await queryManager.GetQueryAsync(queryName);

        if (query == null)
        {
            yield break;
        }

        var parameters = ParseParameters(parametersJson);
        var results = await queryManager.ExecuteQueryAsync(query, parameters);
        if (results?.Items == null)
        {
            yield break;
        }

        foreach (var item in results.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var record = ToJsonObject(item);

            if (record != null)
            {
                yield return record;
            }
        }
    }

    private static Dictionary<string, object> ParseParameters(string parametersJson)
    {
        var parameters = new Dictionary<string, object>();

        if (string.IsNullOrWhiteSpace(parametersJson))
        {
            return parameters;
        }

        try
        {
            if (JsonNode.Parse(parametersJson) is JsonObject obj)
            {
                foreach (var property in obj)
                {
                    parameters[property.Key] = property.Value?.ToString();
                }
            }
        }
        catch
        {
            // Ignore malformed parameter JSON and execute the query without parameters.
        }

        return parameters;
    }

    private static JsonObject ToJsonObject(object item)
    {
        if (item is null)
        {
            return null;
        }

        if (item is JsonObject jsonObject)
        {
            return jsonObject.DeepClone().AsObject();
        }

        var json = JConvert.SerializeObject(item);

        return JsonNode.Parse(json) as JsonObject;
    }
}
