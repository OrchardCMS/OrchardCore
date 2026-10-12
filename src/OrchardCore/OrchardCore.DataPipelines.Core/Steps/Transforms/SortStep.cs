using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Operations;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Sorts the rows by one or more fields, with <see cref="DataSorter"/>: the step reads every row before writing the
/// first one, and writes the rows that don't fit in <see cref="DataPipelineOptions.StepMemoryLimitMegabytes"/> to
/// temporary files. Text is compared ignoring case, empty values come first, and rows with equal keys keep their order.
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

        await using var reader = await DataPipelineRowReader.OpenAsync(context.GetInput(), context.CancellationToken);

        var keys = settings.Keys
            .Select(key => (Index: reader.Fields.IndexOf(key.Field), key.Descending))
            .Where(key => key.Index >= 0)
            .ToArray();

        var sorted = DataSorter.SortAsync(reader.ReadRowsAsync(), DataSorter.CreateComparer(keys), DataPipelineRowReader.CreateSpillOptions(context), context.CancellationToken);

        await DataPipelineRowReader.WriteRowsAsync(context.GetOutput(), reader.Fields, sorted, context.CancellationToken);
    }
}
