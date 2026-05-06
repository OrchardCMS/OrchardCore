using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Joins the current stream with records produced by another source activity in the pipeline.
/// </summary>
public sealed class JoinDataSetsTransform : EtlTransformActivity
{
    public const string InnerJoin = "Inner";
    public const string LeftJoin = "Left";

    public override string Name => nameof(JoinDataSetsTransform);

    public override string DisplayText => "Join Data Sets";

    public string JoinSourceActivityId
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string LeftField
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string RightField
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string JoinType
    {
        get => GetProperty(() => LeftJoin);
        set => SetProperty(value);
    }

    public string RightPrefix
    {
        get => GetProperty(() => "Joined_");
        set => SetProperty(value);
    }

    public override IEnumerable<EtlOutcome> GetPossibleOutcomes()
    {
        return [new EtlOutcome("Done"), new EtlOutcome("Failed")];
    }

    public override async Task<EtlActivityResult> ExecuteAsync(EtlExecutionContext context)
    {
        if (context.DataStream == null)
        {
            return EtlActivityResult.Failure("No left-side data stream available.");
        }

        if (string.IsNullOrWhiteSpace(JoinSourceActivityId))
        {
            return EtlActivityResult.Failure("A join source activity must be selected.");
        }

        var joinSourceRecord = context.Pipeline.Activities.FirstOrDefault(x => x.ActivityId == JoinSourceActivityId);
        if (joinSourceRecord == null)
        {
            return EtlActivityResult.Failure("The selected join source activity could not be found.");
        }

        var joinSourceActivity = context.ActivityLibrary.InstantiateActivity(joinSourceRecord.Name);
        if (joinSourceActivity is not EtlSourceActivity)
        {
            return EtlActivityResult.Failure("The selected join activity must be a source activity.");
        }

        joinSourceActivity.Properties = joinSourceRecord.Properties?.DeepClone() as JsonObject ?? [];

        var joinContext = context.Clone();
        var joinResult = await joinSourceActivity.ExecuteAsync(joinContext);
        if (!joinResult.IsSuccess || joinContext.DataStream == null)
        {
            return EtlActivityResult.Failure(joinResult.ErrorMessage ?? "Unable to execute the selected join source.");
        }

        context.DataStream = TransformAsync(
            context.DataStream,
            joinContext.DataStream,
            LeftField,
            RightField,
            JoinType,
            RightPrefix,
            context.CancellationToken);

        return Outcomes("Done");
    }

    private static async IAsyncEnumerable<JsonObject> TransformAsync(
        IAsyncEnumerable<JsonObject> leftStream,
        IAsyncEnumerable<JsonObject> rightStream,
        string leftField,
        string rightField,
        string joinType,
        string rightPrefix,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(leftField) || string.IsNullOrWhiteSpace(rightField))
        {
            await foreach (var record in leftStream.WithCancellation(cancellationToken))
            {
                yield return record;
            }

            yield break;
        }

        var rightLookup = new Dictionary<string, List<JsonObject>>(StringComparer.OrdinalIgnoreCase);

        await foreach (var record in rightStream.WithCancellation(cancellationToken))
        {
            var key = ResolveFieldValue(record, rightField);
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            if (!rightLookup.TryGetValue(key, out var matches))
            {
                matches = [];
                rightLookup[key] = matches;
            }

            matches.Add(record.DeepClone().AsObject());
        }

        await foreach (var leftRecord in leftStream.WithCancellation(cancellationToken))
        {
            var leftKey = ResolveFieldValue(leftRecord, leftField);
            var leftClone = leftRecord.DeepClone().AsObject();

            if (!string.IsNullOrWhiteSpace(leftKey) && rightLookup.TryGetValue(leftKey, out var rightMatches))
            {
                foreach (var rightRecord in rightMatches)
                {
                    yield return MergeRecords(leftClone, rightRecord, rightPrefix);
                }
            }
            else if (string.Equals(joinType, LeftJoin, StringComparison.OrdinalIgnoreCase))
            {
                yield return leftClone;
            }
        }
    }

    private static JsonObject MergeRecords(JsonObject leftRecord, JsonObject rightRecord, string rightPrefix)
    {
        var merged = leftRecord.DeepClone().AsObject();
        var prefix = rightPrefix ?? string.Empty;

        foreach (var property in rightRecord)
        {
            merged[$"{prefix}{property.Key}"] = property.Value?.DeepClone();
        }

        return merged;
    }

    private static string ResolveFieldValue(JsonObject record, string field)
    {
        var segments = field.Split('.', StringSplitOptions.RemoveEmptyEntries);
        JsonNode current = record;

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

        return current?.ToString();
    }
}
