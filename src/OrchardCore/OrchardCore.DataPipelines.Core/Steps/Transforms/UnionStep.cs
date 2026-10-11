using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Files;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Appends the rows of every step connected to its input, one connection after the other. Fields are matched by name;
/// a field missing from some rows is empty in them, and fields of different types become the type both can hold.
/// </summary>
public sealed class UnionStep : DataPipelineStepType
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "Union";

    private readonly IStringLocalizer S;

    public UnionStep(IStringLocalizer<UnionStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Combine rows"];

    public override LocalizedString Description => S["Appends the rows of several steps."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

    public override string Icon => "fa-solid fa-object-group";

    public override IReadOnlyList<DataPipelinePort> GetInputs(DataPipelineStep step)
        => [new(DataPipelinePort.Input, S["Rows"]) { IsRequired = true, AllowsMany = true }];

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.SetOutputFields(Merge(context.GetInputFieldSets()));

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var input = context.GetInput();
        var output = context.GetOutput();
        var fields = Merge(input.FieldSets);
        IReadOnlyList<DataField> mappedFor = null;
        int[] map = null;

        await foreach (var batch in input.ReadBatchesAsync(context.CancellationToken))
        {
            if (!ReferenceEquals(mappedFor, batch.Fields))
            {
                if (batch.Fields.Any(field => fields.IndexOf(field.Name) < 0))
                {
                    fields = Merge([fields, batch.Fields]);
                }

                map = fields.Select(field => batch.Fields.IndexOf(field.Name)).ToArray();
                mappedFor = batch.Fields;
            }

            var rows = new List<object[]>(batch.Count);

            foreach (var source in batch.Rows)
            {
                var row = new object[fields.Length];

                for (var index = 0; index < fields.Length; index++)
                {
                    if (map[index] >= 0 && map[index] < source.Length)
                    {
                        row[index] = DataValues.Coerce(source[map[index]], fields[index].Type);
                    }
                }

                rows.Add(row);
            }

            await output.WriteAsync(new DataBatch(fields, rows), context.CancellationToken);
        }
    }

    private static DataField[] Merge(IEnumerable<IReadOnlyList<DataField>> sets)
    {
        var order = new List<string>();
        var byName = new Dictionary<string, (DataField Field, List<DataFieldType> Types)>(StringComparer.Ordinal);

        foreach (var set in sets)
        {
            foreach (var field in set)
            {
                if (!byName.TryGetValue(field.Name, out var entry))
                {
                    entry = (field, []);
                    byName[field.Name] = entry;
                    order.Add(field.Name);
                }

                entry.Types.Add(field.Type);
            }
        }

        return order
            .Select(name =>
            {
                var (field, types) = byName[name];
                var type = DataFileValues.InferType(types);

                return type == field.Type ? field : new DataField(field.Name, field.DisplayName, type, field.Group);
            })
            .ToArray();
    }
}
