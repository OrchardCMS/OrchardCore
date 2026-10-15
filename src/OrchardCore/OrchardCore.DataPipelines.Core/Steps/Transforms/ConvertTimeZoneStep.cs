using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataSources;
using OrchardCore.Localization;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Converts date and time fields from UTC, as Orchard Core stores dates, to the local date and time of a time zone, or
/// from the local date and time of a time zone to UTC. The time zone is the site's, the one of the user the pipeline
/// runs for, or a chosen one, resolved by <see cref="IDataTimeZoneResolver"/>, and the conversion follows its daylight
/// saving time with <see cref="IClock"/>. A local time that a change of offset skips, or repeats, takes the offset in
/// effect before the change.
/// </summary>
public sealed class ConvertTimeZoneStep : DataPipelineStepType<ConvertTimeZoneStep, ConvertTimeZoneStepSettings>, IDataPipelineStepType
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "ConvertTimeZone";

    private readonly IClock _clock;
    private readonly IDataTimeZoneResolver _timeZoneResolver;
    private readonly IStringLocalizer S;

    public ConvertTimeZoneStep(
        IClock clock,
        IDataTimeZoneResolver timeZoneResolver,
        IStringLocalizer<ConvertTimeZoneStep> localizer)
    {
        _clock = clock;
        _timeZoneResolver = timeZoneResolver;
        S = localizer;
    }

    public static string Name => StepName;

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ConvertTimeZoneStep>("Convert time zone");

    public static LocalizationSource Description { get; } = LocalizationSource.Create<ConvertTimeZoneStep>("Converts dates and times from UTC to a time zone, or from a time zone to UTC.");

    public static DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

    public static string Icon => "fa-solid fa-earth-americas";

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var fields = context.GetInputFields();

        foreach (var name in settings.Fields)
        {
            var index = fields.IndexOf(name);

            if (index < 0)
            {
                context.AddError(S["The input has no field named '{0}'.", name ?? string.Empty]);
            }
            else if (fields[index].Type != DataFieldType.DateTime)
            {
                context.AddError(S["The field '{0}' isn't a date and time, so it has no time zone.", name]);
            }
        }

        if (settings.Fields.Count == 0 && fields.Count > 0 && !fields.Any(field => field.Type == DataFieldType.DateTime))
        {
            context.AddWarning(S["The input has no date and time field to convert."]);
        }

        if (settings.TimeZone == TimeZoneSource.Specific && _timeZoneResolver.GetTimeZone(settings.TimeZoneId) is null)
        {
            context.AddError(string.IsNullOrEmpty(settings.TimeZoneId)
                ? S["Choose a time zone."]
                : S["There is no time zone named '{0}'.", settings.TimeZoneId]);
        }

        context.SetOutputFields(fields);

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var timeZone = await GetTimeZoneAsync(settings, context)
            ?? throw new InvalidOperationException(S["There is no time zone named '{0}'.", settings.TimeZoneId ?? string.Empty]);
        var output = context.GetOutput();

        int[] indexes = null;
        IReadOnlyList<DataField> plannedFor = null;

        await foreach (var batch in context.GetInput().ReadBatchesAsync(context.CancellationToken))
        {
            if (!ReferenceEquals(plannedFor, batch.Fields))
            {
                indexes = GetIndexes(batch.Fields, settings);
                plannedFor = batch.Fields;
            }

            if (indexes.Length == 0)
            {
                await output.WriteAsync(batch, context.CancellationToken);

                continue;
            }

            var rows = new List<object[]>(batch.Count);

            foreach (var source in batch.Rows)
            {
                var row = (object[])source.Clone();

                foreach (var index in indexes)
                {
                    if (index < row.Length && DataValues.Coerce(row[index], DataFieldType.DateTime) is DateTime value)
                    {
                        row[index] = settings.Conversion == TimeZoneConversion.ToUtc
                            ? ToUtc(value, timeZone)
                            : FromUtc(value, timeZone);
                    }
                }

                rows.Add(row);
            }

            await output.WriteAsync(new DataBatch(batch.Fields, rows), context.CancellationToken);
        }
    }

    /// <summary>
    /// Converts a date and time in UTC to the local date and time of a time zone.
    /// </summary>
    /// <param name="utc">The date and time in UTC. Its kind is ignored.</param>
    /// <param name="timeZone">The time zone.</param>
    /// <returns>The local date and time, of the <see cref="DateTimeKind.Unspecified"/> kind.</returns>
    public DateTime FromUtc(DateTime utc, ITimeZone timeZone)
    {
        var local = _clock.ConvertToTimeZone(new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)), timeZone);

        return DateTime.SpecifyKind(local.DateTime, DateTimeKind.Unspecified);
    }

    /// <summary>
    /// Converts the local date and time of a time zone to UTC.
    /// </summary>
    /// <param name="local">The local date and time. Its kind is ignored.</param>
    /// <param name="timeZone">The time zone.</param>
    /// <returns>The date and time in UTC.</returns>
    public DateTime ToUtc(DateTime local, ITimeZone timeZone)
    {
        var wallClock = DateTime.SpecifyKind(local, DateTimeKind.Utc);
        var expected = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);

        // The offsets a day before and a day after the local time are the only ones it can have, as time zones change
        // their offset at most once in two days. The first one that maps back to the local time is taken, which is
        // the offset before the change for a local time that is repeated, and the offset before the change is kept
        // for a local time that is skipped.
        var before = GetOffset(wallClock.AddDays(-1), timeZone);
        var after = GetOffset(wallClock.AddDays(1), timeZone);

        TimeSpan[] offsets = before == after ? [before] : [before, after];

        foreach (var offset in offsets)
        {
            var utc = wallClock - offset;

            if (FromUtc(utc, timeZone) == expected)
            {
                return utc;
            }
        }

        return wallClock - before;
    }

    private TimeSpan GetOffset(DateTime utc, ITimeZone timeZone)
    {
        return _clock.ConvertToTimeZone(new DateTimeOffset(utc), timeZone).Offset;
    }

    private Task<ITimeZone> GetTimeZoneAsync(ConvertTimeZoneStepSettings settings, DataPipelineStepContext context)
    {
        return settings.TimeZone switch
        {
            TimeZoneSource.User => _timeZoneResolver.GetUserTimeZoneAsync(context.Run.User),
            TimeZoneSource.Specific => Task.FromResult(_timeZoneResolver.GetTimeZone(settings.TimeZoneId)),
            _ => _timeZoneResolver.GetSiteTimeZoneAsync(),
        };
    }

    private static int[] GetIndexes(IReadOnlyList<DataField> fields, ConvertTimeZoneStepSettings settings)
    {
        if (settings.Fields.Count == 0)
        {
            return Enumerable.Range(0, fields.Count).Where(index => fields[index].Type == DataFieldType.DateTime).ToArray();
        }

        return settings.Fields
            .Select(name => fields.IndexOf(name))
            .Where(index => index >= 0 && fields[index].Type == DataFieldType.DateTime)
            .ToArray();
    }
}
