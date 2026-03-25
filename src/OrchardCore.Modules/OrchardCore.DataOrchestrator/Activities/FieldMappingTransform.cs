using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Maps and renames fields in records based on configurable source-to-target mappings.
/// </summary>
public sealed class FieldMappingTransform : EtlTransformActivity
{
    public override string Name => nameof(FieldMappingTransform);

    public override string DisplayText => "Field Mapping";

    public string MappingsJson
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
        var input = context.DataStream;
        var mappingsJson = MappingsJson;

        context.DataStream = TransformAsync(input, mappingsJson, context.CancellationToken);

        return Task.FromResult(Outcomes("Done"));
    }

    private static async IAsyncEnumerable<JsonObject> TransformAsync(
        IAsyncEnumerable<JsonObject> input,
        string mappingsJson,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (input == null)
        {
            yield break;
        }

        List<(string Source, string Target)> mappings = [];

        if (!string.IsNullOrWhiteSpace(mappingsJson))
        {
            try
            {
                var array = JsonNode.Parse(mappingsJson) as JsonArray;
                if (array != null)
                {
                    foreach (var m in array)
                    {
                        var source = m?["source"]?.GetValue<string>();
                        var target = m?["target"]?.GetValue<string>();
                        if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(target))
                        {
                            mappings.Add((source, target));
                        }
                    }
                }
            }
            catch
            {
                // If mappings can't be parsed, pass through
            }
        }

        await foreach (var record in input.WithCancellation(cancellationToken))
        {
            if (mappings.Count == 0)
            {
                yield return record;
                continue;
            }

            var result = new JsonObject();

            foreach (var (source, target) in mappings)
            {
                var value = ResolveJsonPath(record, source);
                if (value is not null)
                {
                    result[target] = value.DeepClone();
                }
            }

            yield return result;
        }
    }

    private static JsonNode ResolveJsonPath(JsonObject root, string path)
    {
        var segments = path.Split('.');
        JsonNode current = root;

        foreach (var segment in segments)
        {
            if (current is JsonObject obj && obj.TryGetPropertyValue(segment, out var next))
            {
                current = next;
            }
            else
            {
                return null;
            }
        }

        return current;
    }
}
