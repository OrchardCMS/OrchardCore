using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Formats or converts a field value and writes it back to the current record.
/// </summary>
public sealed class FormatValueTransform : EtlTransformActivity
{
    public const string CurrencyFormat = "Currency";
    public const string NumberFormat = "Number";
    public const string DateTimeFormat = "DateTime";
    public const string ConvertUtcToTimeZoneFormat = "ConvertUtcToTimeZone";
    public const string UppercaseFormat = "Uppercase";
    public const string LowercaseFormat = "Lowercase";

    public override string Name => nameof(FormatValueTransform);

    public override string DisplayText => "Format Value";

    public string Field
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string OutputField
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string FormatType
    {
        get => GetProperty(() => DateTimeFormat);
        set => SetProperty(value);
    }

    public string FormatString
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string Culture
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string TimeZoneId
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
        if (!TryGetCultureInfo(Culture, out var cultureInfo, out var errorMessage))
        {
            return Task.FromResult(EtlActivityResult.Failure(errorMessage));
        }

        if (!TryGetTimeZoneInfo(FormatType, TimeZoneId, out var timeZoneInfo, out errorMessage))
        {
            return Task.FromResult(EtlActivityResult.Failure(errorMessage));
        }

        context.DataStream = TransformAsync(
            context.DataStream,
            Field,
            OutputField,
            FormatType,
            FormatString,
            cultureInfo,
            timeZoneInfo,
            context.CancellationToken);

        return Task.FromResult(Outcomes("Done"));
    }

    private static async IAsyncEnumerable<JsonObject> TransformAsync(
        IAsyncEnumerable<JsonObject> input,
        string field,
        string outputField,
        string formatType,
        string formatString,
        CultureInfo cultureInfo,
        TimeZoneInfo timeZoneInfo,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (input == null)
        {
            yield break;
        }

        await foreach (var record in input.WithCancellation(cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                yield return record;
                continue;
            }

            var sourceValue = ResolveFieldValue(record, field);
            if (sourceValue == null)
            {
                yield return record;
                continue;
            }

            var cloned = record.DeepClone().AsObject();
            var destinationField = string.IsNullOrWhiteSpace(outputField) ? field : outputField;
            SetFieldValue(cloned, destinationField, ApplyFormat(sourceValue, formatType, formatString, cultureInfo, timeZoneInfo));
            yield return cloned;
        }
    }

    private static bool TryGetCultureInfo(string culture, out CultureInfo cultureInfo, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            cultureInfo = CultureInfo.InvariantCulture;
            errorMessage = null;

            return true;
        }

        try
        {
            cultureInfo = CultureInfo.GetCultureInfo(culture);
            errorMessage = null;

            return true;
        }
        catch (CultureNotFoundException)
        {
            cultureInfo = null;
            errorMessage = $"The culture '{culture}' is not valid.";

            return false;
        }
    }

    private static bool TryGetTimeZoneInfo(
        string formatType,
        string timeZoneId,
        out TimeZoneInfo timeZoneInfo,
        out string errorMessage)
    {
        timeZoneInfo = null;
        errorMessage = null;

        if (!string.Equals(formatType, ConvertUtcToTimeZoneFormat, StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(timeZoneId))
        {
            return true;
        }

        try
        {
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            errorMessage = $"The time zone '{timeZoneId}' could not be found.";

            return false;
        }
        catch (InvalidTimeZoneException)
        {
            errorMessage = $"The time zone '{timeZoneId}' is invalid.";

            return false;
        }
    }

    private static string ApplyFormat(
        string sourceValue,
        string formatType,
        string formatString,
        CultureInfo cultureInfo,
        TimeZoneInfo timeZoneInfo)
    {
        return formatType switch
        {
            CurrencyFormat when decimal.TryParse(sourceValue, out var currencyValue) => currencyValue.ToString(formatString ?? "C", cultureInfo),
            NumberFormat when decimal.TryParse(sourceValue, out var numberValue) => numberValue.ToString(formatString ?? "N", cultureInfo),
            DateTimeFormat when DateTime.TryParse(sourceValue, out var dateTimeValue) => dateTimeValue.ToString(formatString ?? "u", cultureInfo),
            ConvertUtcToTimeZoneFormat when DateTime.TryParse(sourceValue, out var utcValue) => ConvertUtcToTimeZone(utcValue, timeZoneInfo, formatString, cultureInfo),
            UppercaseFormat => sourceValue.ToUpper(cultureInfo),
            LowercaseFormat => sourceValue.ToLower(cultureInfo),
            _ => sourceValue,
        };
    }

    private static string ConvertUtcToTimeZone(DateTime utcValue, TimeZoneInfo timeZone, string formatString, CultureInfo cultureInfo)
    {
        if (timeZone == null)
        {
            return utcValue.ToString(formatString ?? "u", cultureInfo);
        }

        var normalized = utcValue.Kind == DateTimeKind.Utc ? utcValue : DateTime.SpecifyKind(utcValue, DateTimeKind.Utc);
        var converted = TimeZoneInfo.ConvertTimeFromUtc(normalized, timeZone);

        return converted.ToString(formatString ?? "G", cultureInfo);
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

    private static void SetFieldValue(JsonObject root, string field, string value)
    {
        var segments = field.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var current = root;

        for (var i = 0; i < segments.Length - 1; i++)
        {
            if (current[segments[i]] is not JsonObject next)
            {
                next = [];
                current[segments[i]] = next;
            }

            current = next;
        }

        current[segments[^1]] = value;
    }
}
