using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Operations;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Groups the rows by the values of some fields and summarizes each group, such as the number of orders and the
/// revenue per customer. Without group fields, it summarizes every row into one. Groups compare their values exactly;
/// the step keeps one entry per group in memory, and the values of a median.
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

    public override LocalizedString Description => S["Groups rows and computes counts, sums, averages, minimums and maximums."];

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
        var limit = DataPipelineFormulas.GetOptions(context.Services).MaxRowsInMemory;
        var groups = new Dictionary<string, Group>(StringComparer.Ordinal);
        var order = new List<Group>();

        Plan plan = null;
        IReadOnlyList<DataField> plannedFor = null;

        await foreach (var batch in context.GetInput().ReadBatchesAsync(context.CancellationToken))
        {
            if (!ReferenceEquals(plannedFor, batch.Fields))
            {
                plan = BuildPlan(batch.Fields, settings, out var errors);

                if (errors.Count > 0)
                {
                    throw new InvalidOperationException(errors[0]);
                }

                plannedFor = batch.Fields;
            }

            foreach (var row in batch.Rows)
            {
                var key = DataPipelineKeys.Build(row, plan.GroupIndexes, ignoreCase: false);

                if (!groups.TryGetValue(key, out var group))
                {
                    if (groups.Count >= limit)
                    {
                        throw new InvalidOperationException(S["The step can summarize {0} groups at most.", limit]);
                    }

                    group = new Group(plan.GroupIndexes.Select(index => row[index]).ToArray(), plan.Measures.Select(measure => new Accumulator(measure.Function)).ToArray());
                    groups[key] = group;
                    order.Add(group);
                }

                for (var index = 0; index < plan.Measures.Count; index++)
                {
                    var measure = plan.Measures[index];
                    group.Accumulators[index].Add(measure.SourceIndex < 0 ? true : row[measure.SourceIndex]);
                }
            }
        }

        // Without group fields, an empty input still has one summary row, like a SQL aggregate.
        if (order.Count == 0 && settings.GroupBy.Count == 0)
        {
            plan ??= BuildPlan(context.GetInput().Fields, settings, out _);
            order.Add(new Group([], plan.Measures.Select(measure => new Accumulator(measure.Function)).ToArray()));
        }

        if (plan is null)
        {
            return;
        }

        var output = context.GetOutput();

        foreach (var chunk in order.Chunk(DataSourceQuery.DefaultBatchSize))
        {
            var rows = chunk
                .Select(group => group.Values
                    .Concat(group.Accumulators.Select((accumulator, index) => DataValues.Coerce(accumulator.Result(), plan.Fields[plan.GroupIndexes.Length + index].Type)))
                    .ToArray())
                .ToList();

            await output.WriteAsync(new DataBatch(plan.Fields, rows), context.CancellationToken);
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

    private sealed record Group(object[] Values, Accumulator[] Accumulators);

    /// <summary>
    /// Computes one aggregate as the rows arrive, without keeping them, except for a median.
    /// </summary>
    private sealed class Accumulator
    {
        private readonly DataAggregate _function;
        private readonly HashSet<string> _distinct;
        private readonly List<object> _values;
        private long _count;
        private decimal _sum;
        private bool _hasSum;
        private object _extreme;

        public Accumulator(DataAggregate function)
        {
            _function = function;

            if (function == DataAggregate.CountDistinct)
            {
                _distinct = new HashSet<string>(StringComparer.Ordinal);
            }
            else if (function == DataAggregate.Median)
            {
                _values = [];
            }
        }

        public void Add(object value)
        {
            if (DataValues.IsEmpty(value))
            {
                return;
            }

            _count++;

            switch (_function)
            {
                case DataAggregate.CountDistinct:
                    _distinct.Add(DataPipelineKeys.Build([value], [0], ignoreCase: false));
                    break;

                case DataAggregate.Median:
                    _values.Add(value);
                    break;

                case DataAggregate.Sum or DataAggregate.Average:
                    if (DataValues.Coerce(value, DataFieldType.Decimal) is decimal number)
                    {
                        _sum += number;
                        _hasSum = true;
                    }

                    break;

                case DataAggregate.Min:
                    if (_extreme is null || DataValues.Compare(value, _extreme) < 0)
                    {
                        _extreme = value;
                    }

                    break;

                case DataAggregate.Max:
                    if (_extreme is null || DataValues.Compare(value, _extreme) > 0)
                    {
                        _extreme = value;
                    }

                    break;
            }
        }

        public object Result()
            => _function switch
            {
                DataAggregate.Count => _count,
                DataAggregate.CountDistinct => (long)_distinct.Count,
                DataAggregate.Sum => _hasSum ? _sum : null,
                DataAggregate.Average => _hasSum && _count > 0 ? _sum / _count : null,
                DataAggregate.Min or DataAggregate.Max => _extreme,
                DataAggregate.Median => DataAggregations.Median(_values),
                _ => null,
            };
    }
}
