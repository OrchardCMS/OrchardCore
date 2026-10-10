using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Sorts the rows by one or more fields. The step holds every row in memory before writing the first one, up to
/// <see cref="DataPipelineOptions.MaxRowsInMemory"/>. Text is compared ignoring case, and empty values come first.
/// </summary>
public sealed class SortStep : DataPipelineStepType<SortStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "Sort";

    private readonly IStringLocalizer S;

    public SortStep(IStringLocalizer<SortStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Sort rows"];

    public override LocalizedString Description => S["Sorts the rows by one or more fields."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

    public override string Icon => "fa-solid fa-arrow-down-wide-short";

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var fields = context.GetInputFields();
        var settings = GetSettings(context.Step);

        if (settings.Keys.Count == 0)
        {
            context.AddError(S["Choose the fields to sort by."]);
        }

        foreach (var key in settings.Keys.Where(key => fields.IndexOf(key.Field) < 0))
        {
            context.AddError(S["The input has no field named '{0}'.", key.Field ?? string.Empty]);
        }

        context.SetOutputFields(fields);

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var limit = DataPipelineFormulas.GetOptions(context.Services).MaxRowsInMemory;
        var rows = new List<object[]>();
        IReadOnlyList<DataField> fields = context.GetInput().Fields;

        await foreach (var batch in context.GetInput().ReadBatchesAsync(context.CancellationToken))
        {
            fields = batch.Fields;
            rows.AddRange(batch.Rows);

            if (rows.Count > limit)
            {
                throw new InvalidOperationException(S["The step can sort {0} rows at most.", limit]);
            }
        }

        var keys = settings.Keys
            .Select(key => (Index: fields.IndexOf(key.Field), key.Descending))
            .Where(key => key.Index >= 0)
            .ToArray();

        // A stable sort keeps the order of rows with equal keys.
        var sorted = rows
            .Select((row, position) => (Row: row, Position: position))
            .Order(Comparer<(object[] Row, int Position)>.Create((left, right) =>
            {
                foreach (var (index, descending) in keys)
                {
                    var result = DataValues.Compare(left.Row[index], right.Row[index]);

                    if (result != 0)
                    {
                        return descending ? -result : result;
                    }
                }

                return left.Position.CompareTo(right.Position);
            }))
            .Select(item => item.Row);

        var output = context.GetOutput();

        foreach (var chunk in sorted.Chunk(DataSourceQuery.DefaultBatchSize))
        {
            await output.WriteAsync(new DataBatch(fields, chunk), context.CancellationToken);
        }
    }
}
