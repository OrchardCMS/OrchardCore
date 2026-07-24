using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Filters records based on a configurable field condition.
/// </summary>
public sealed class FilterTransform : EtlTransformActivity
{
    public override string Name => nameof(FilterTransform);

    public override string DisplayText => "Filter";

    public string Field
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string Operator
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string Value
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

        context.DataStream = TransformAsync(input, Field, Operator, Value, context.CancellationToken);

        return Task.FromResult(Outcomes("Done"));
    }

    private static async IAsyncEnumerable<JsonObject> TransformAsync(
        IAsyncEnumerable<JsonObject> input,
        string field,
        string op,
        string value,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (input == null)
        {
            yield break;
        }

        if (string.IsNullOrEmpty(field) || string.IsNullOrEmpty(op))
        {
            await foreach (var record in input.WithCancellation(cancellationToken))
            {
                yield return record;
            }

            yield break;
        }

        await foreach (var record in input.WithCancellation(cancellationToken))
        {
            var fieldValue = ResolveFieldValue(record, field);

            if (Evaluate(fieldValue, op, value))
            {
                yield return record;
            }
        }
    }

    private static string ResolveFieldValue(JsonObject record, string field)
    {
        var segments = field.Split('.');
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

    private static bool Evaluate(string fieldValue, string op, string compareValue)
    {
        if (fieldValue is null)
        {
            return op.Equals("not_equals", StringComparison.OrdinalIgnoreCase) ||
                   op.Equals("not_contains", StringComparison.OrdinalIgnoreCase);
        }

        return op.ToUpperInvariant() switch
        {
            "EQUALS" => string.Equals(fieldValue, compareValue, StringComparison.OrdinalIgnoreCase),
            "NOT_EQUALS" => !string.Equals(fieldValue, compareValue, StringComparison.OrdinalIgnoreCase),
            "CONTAINS" => fieldValue.Contains(compareValue ?? string.Empty, StringComparison.OrdinalIgnoreCase),
            "NOT_CONTAINS" => !fieldValue.Contains(compareValue ?? string.Empty, StringComparison.OrdinalIgnoreCase),
            "STARTS_WITH" => fieldValue.StartsWith(compareValue ?? string.Empty, StringComparison.OrdinalIgnoreCase),
            "ENDS_WITH" => fieldValue.EndsWith(compareValue ?? string.Empty, StringComparison.OrdinalIgnoreCase),
            "GREATER_THAN" => CompareValues(fieldValue, compareValue) > 0,
            "GREATER_THAN_OR_EQUAL" => CompareValues(fieldValue, compareValue) >= 0,
            "LESS_THAN" => CompareValues(fieldValue, compareValue) < 0,
            "LESS_THAN_OR_EQUAL" => CompareValues(fieldValue, compareValue) <= 0,
            "IS_EMPTY" => string.IsNullOrWhiteSpace(fieldValue),
            "IS_NOT_EMPTY" => !string.IsNullOrWhiteSpace(fieldValue),
            _ => true,
        };
    }

    private static int CompareValues(string fieldValue, string compareValue)
    {
        if (decimal.TryParse(fieldValue, out var fieldNumber) && decimal.TryParse(compareValue, out var compareNumber))
        {
            return fieldNumber.CompareTo(compareNumber);
        }

        if (DateTime.TryParse(fieldValue, out var fieldDate) && DateTime.TryParse(compareValue, out var compareDate))
        {
            return fieldDate.CompareTo(compareDate);
        }

        return string.Compare(fieldValue, compareValue, StringComparison.OrdinalIgnoreCase);
    }
}
