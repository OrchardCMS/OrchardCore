using Microsoft.Extensions.Localization;
using OrchardCore.DataSources.Operations;
using OrchardCore.DataSources;
using OrchardCore.Localization;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Removes the rows whose key fields hold the same values as a row before them. Without key fields, a row is a
/// duplicate when every field is equal. Values compare exactly, and the rows keep their order. The step keeps the keys
/// it saw in memory, up to <see cref="DataPipelineOptions.StepMemoryLimitMegabytes"/>, with <see cref="DataDistinct"/>.
/// </summary>
public sealed class DistinctStep : DataPipelineStepType<DistinctStep, DistinctStepSettings>, IDataPipelineStepType
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

    public static string Name => StepName;

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<DistinctStep>("Remove duplicates");

    public static LocalizationSource Description { get; } = LocalizationSource.Create<DistinctStep>("Keeps the first row of each set of equal values.");

    public static DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

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

        await using var reader = await DataPipelineRowReader.OpenAsync(context.GetInput(), context.CancellationToken);

        var keys = settings.Fields.Count == 0
            ? Enumerable.Range(0, reader.Fields.Count).ToArray()
            : settings.Fields.Select(reader.Fields.IndexOf).Where(index => index >= 0).ToArray();

        var distinct = DataDistinct.DistinctAsync(reader.ReadRowsAsync(), keys, ignoreCase: false, DataPipelineRowReader.CreateSpillOptions(context), context.CancellationToken);

        await DataPipelineRowReader.WriteRowsAsync(context.GetOutput(), reader.Fields, distinct, context.CancellationToken);
    }
}
