using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Operations;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Groups the rows by the values of some fields and summarizes each group, such as the number of orders and the
/// revenue per customer. Without group fields, it summarizes every row into one. Groups compare their values exactly,
/// and come out in the order they first appear. The step keeps the groups in memory, up to
/// <see cref="DataPipelineOptions.StepMemoryLimitMegabytes"/>, with <see cref="DataAggregator"/>; the values of a median
/// and of a distinct count stay in memory with their group.
/// </summary>
public sealed class AggregateStep : DataPipelineStepType<AggregateStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "Aggregate";

    private readonly IStringLocalizer S;

    public AggregateStep(IStringLocalizer<AggregateStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Group and summarize"];

    public override LocalizedString Description
    {
        get
        {
            return S["Groups rows and computes counts, sums, averages, minimums and maximums."];
        }
    }

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

    public override string Icon => "fa-solid fa-layer-group";

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var plan = BuildPlan(context.GetInputFields(), GetSettings(context.Step), out var errors);

        foreach (var error in errors)
        {
            context.AddError(error);
        }

        context.SetOutputFields(plan.Fields);

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);

        await using var reader = await DataPipelineRowReader.OpenAsync(context.GetInput(), context.CancellationToken);

        var plan = BuildPlan(reader.Fields, settings, out var errors);

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(errors[0]);
        }

        var measures = plan.Measures.Select(measure => new DataMeasure(measure.Function, measure.SourceIndex)).ToArray();
        var groups = DataAggregator.AggregateAsync(reader.ReadRowsAsync(), plan.GroupIndexes, measures, DataPipelineRowReader.CreateSpillOptions(context), context.CancellationToken);

        await DataPipelineRowReader.WriteRowsAsync(context.GetOutput(), plan.Fields, Coerce(groups, plan), context.CancellationToken);
    }

    private static async IAsyncEnumerable<object[]> Coerce(IAsyncEnumerable<object[]> groups, Plan plan)
    {
        await foreach (var group in groups)
        {
            for (var index = plan.GroupIndexes.Length; index < group.Length; index++)
            {
                group[index] = DataValues.Coerce(group[index], plan.Fields[index].Type);
            }

            yield return group;
        }
    }

    private Plan BuildPlan(IReadOnlyList<DataField> input, AggregateStepSettings settings, out List<string> errors)
    {
        errors = [];

        var fields = new List<DataField>();
        var groupIndexes = new List<int>();

        foreach (var name in settings.GroupBy)
        {
            var index = input.IndexOf(name);

            if (index < 0)
            {
                errors.Add(S["The input has no field named '{0}'.", name ?? string.Empty]);

                continue;
            }

            groupIndexes.Add(index);
            fields.Add(input[index]);
        }

        var measures = new List<Measure>();

        if (settings.Measures.Count == 0)
        {
            errors.Add(S["Add a value to compute, such as the number of rows."]);
        }

        foreach (var measure in settings.Measures)
        {
            var name = measure.Name?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                errors.Add(S["Enter the name of each computed value."]);

                continue;
            }

            if (fields.IndexOf(name) >= 0)
            {
                errors.Add(S["Two fields are named '{0}'.", name]);

                continue;
            }

            var sourceIndex = -1;
            var sourceType = DataFieldType.Integer;

            if (!string.IsNullOrEmpty(measure.Field))
            {
                sourceIndex = input.IndexOf(measure.Field);

                if (sourceIndex < 0)
                {
                    errors.Add(S["The input has no field named '{0}'.", measure.Field]);

                    continue;
                }

                sourceType = input[sourceIndex].Type;
            }
            else if (measure.Function != DataAggregate.Count)
            {
                errors.Add(S["Choose the field of '{0}'.", name]);

                continue;
            }

            if (measure.Function is DataAggregate.None || !DataAggregations.Supports(measure.Function, sourceType))
            {
                errors.Add(S["'{0}' can't be computed on a field of type {1}.", name, sourceType]);

                continue;
            }

            fields.Add(new DataField(name, name, DataAggregations.GetResultType(measure.Function, sourceType), S["Summary"]));
            measures.Add(new Measure(measure.Function, sourceIndex));
        }

        return new Plan(fields.ToArray(), [.. groupIndexes], measures);
    }

    private sealed record Measure(DataAggregate Function, int SourceIndex);

    private sealed record Plan(DataField[] Fields, int[] GroupIndexes, List<Measure> Measures);
}
