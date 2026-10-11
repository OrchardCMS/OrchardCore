using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Removes the rows whose key fields hold the same values as a row before them. Without key fields, a row is a
/// duplicate when every field is equal. Values compare exactly; the step keeps one key per distinct row in memory.
/// </summary>
public sealed class DistinctStep : DataPipelineStepType<DistinctStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "Distinct";

    private readonly IStringLocalizer S;

    public DistinctStep(IStringLocalizer<DistinctStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Remove duplicates"];

    public override LocalizedString Description => S["Keeps the first row of each set of equal values."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

    public override string Icon => "fa-solid fa-clone";

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var fields = context.GetInputFields();

        foreach (var name in GetSettings(context.Step).Fields.Where(name => fields.IndexOf(name) < 0))
        {
            context.AddError(S["The input has no field named '{0}'.", name ?? string.Empty]);
        }

        context.SetOutputFields(fields);

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var limit = DataPipelineFormulas.GetOptions(context.Services).MaxRowsInMemory;
        var output = context.GetOutput();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        await foreach (var batch in context.GetInput().ReadBatchesAsync(context.CancellationToken))
        {
            var keys = settings.Fields.Count == 0
                ? Enumerable.Range(0, batch.Fields.Count).ToArray()
                : settings.Fields.Select(batch.IndexOf).Where(index => index >= 0).ToArray();

            var rows = new List<object[]>();

            foreach (var row in batch.Rows)
            {
                if (seen.Add(DataPipelineKeys.Build(row, keys, ignoreCase: false)))
                {
                    if (seen.Count > limit)
                    {
                        throw new InvalidOperationException(S["The step can keep {0} distinct rows at most.", limit]);
                    }

                    rows.Add(row);
                }
            }

            if (rows.Count > 0)
            {
                await output.WriteAsync(new DataBatch(batch.Fields, rows), context.CancellationToken);
            }
        }
    }
}
