using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Extracts records from a JSON array provided in the configuration.
/// </summary>
public sealed class JsonSource : EtlSourceActivity
{
    public override string Name => nameof(JsonSource);

    public override string DisplayText => "JSON Data";

    public string Data
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
        context.DataStream = ExtractAsync(Data);

        return Task.FromResult(Outcomes("Done"));
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators
    private static async IAsyncEnumerable<JsonObject> ExtractAsync(string data)
#pragma warning restore CS1998
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            yield break;
        }

        JsonNode parsed;

        try
        {
            parsed = JsonNode.Parse(data);
        }
        catch
        {
            yield break;
        }

        if (parsed is not JsonArray array)
        {
            yield break;
        }

        foreach (var item in array)
        {
            if (item is JsonObject obj)
            {
                yield return obj.Deserialize<JsonObject>();
            }
        }
    }
}
