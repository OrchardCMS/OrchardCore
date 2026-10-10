using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Combines each row of the left input with the rows of the right input whose key fields hold the same values, like a
/// SQL join. The step holds the right input in memory, up to <see cref="DataPipelineOptions.MaxRowsInMemory"/>, and
/// streams the left one, so connect the smaller data set to the right. A field of the right input whose name the left
/// input already has gets a prefix.
/// </summary>
public sealed class JoinStep : DataPipelineStepType<JoinStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "Join";

    /// <summary>
    /// The left input, which is streamed.
    /// </summary>
    public const string Left = "Left";

    /// <summary>
    /// The right input, which is held in memory.
    /// </summary>
    public const string Right = "Right";

    private readonly IStringLocalizer S;

    public JoinStep(IStringLocalizer<JoinStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Join"];

    public override LocalizedString Description => S["Combines the rows of two inputs that share key values."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

    public override string Icon => "fa-solid fa-code-merge";

    public override IReadOnlyList<DataPipelinePort> GetInputs(DataPipelineStep step)
        =>
        [
            new(Left, S["Left"]) { IsRequired = true },
            new(Right, S["Right"]) { IsRequired = true },
        ];

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var plan = BuildPlan(context.GetInputFields(Left), context.GetInputFields(Right), GetSettings(context.Step), out var errors);

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
        var left = context.GetInput(Left);
        var right = context.GetInput(Right);
        var output = context.GetOutput();

        // Read the right input first: it builds the lookup the left rows are matched against.
        var rightFields = right.Fields;
        var rightRows = new List<object[]>();

        await foreach (var batch in right.ReadBatchesAsync(context.CancellationToken))
        {
            rightFields = batch.Fields;
            rightRows.AddRange(batch.Rows);

            if (rightRows.Count > limit)
            {
                throw new InvalidOperationException(S["The right input of a join can hold {0} rows at most. Connect the smaller data set to the right.", limit]);
            }
        }

        Plan plan = null;
        IReadOnlyList<DataField> plannedFor = null;
        Dictionary<string, List<int>> lookup = null;
        var matched = new bool[rightRows.Count];
        var keepUnmatchedLeft = settings.JoinType is DataJoinType.Left or DataJoinType.Full;

        await foreach (var batch in left.ReadBatchesAsync(context.CancellationToken))
        {
            if (!ReferenceEquals(plannedFor, batch.Fields))
            {
                plan = BuildPlan(batch.Fields, rightFields, settings, out var errors);

                if (errors.Count > 0)
                {
                    throw new InvalidOperationException(errors[0]);
                }

                plannedFor = batch.Fields;
                lookup ??= BuildLookup(rightRows, plan.RightKeys, settings.IgnoreCase);
            }

            var rows = new List<object[]>();

            foreach (var row in batch.Rows)
            {
                var found = false;

                if (!DataPipelineKeys.HasEmptyKey(row, plan.LeftKeys) &&
                    lookup.TryGetValue(DataPipelineKeys.Build(row, plan.LeftKeys, settings.IgnoreCase), out var indexes))
                {
                    foreach (var index in indexes)
                    {
                        matched[index] = true;
                        rows.Add(Combine(row, rightRows[index], plan));
                        found = true;
                    }
                }

                if (!found && keepUnmatchedLeft)
                {
                    rows.Add(Combine(row, null, plan));
                }
            }

            if (rows.Count > 0)
            {
                await output.WriteAsync(new DataBatch(plan.Fields, rows), context.CancellationToken);
            }
        }

        if (settings.JoinType is not (DataJoinType.Right or DataJoinType.Full))
        {
            return;
        }

        plan ??= BuildPlan(left.Fields, rightFields, settings, out _);

        var unmatched = new List<object[]>();

        for (var index = 0; index < rightRows.Count; index++)
        {
            if (!matched[index])
            {
                unmatched.Add(Combine(null, rightRows[index], plan));
            }
        }

        foreach (var chunk in unmatched.Chunk(DataSourceQuery.DefaultBatchSize))
        {
            await output.WriteAsync(new DataBatch(plan.Fields, chunk), context.CancellationToken);
        }
    }

    private static Dictionary<string, List<int>> BuildLookup(List<object[]> rows, int[] keys, bool ignoreCase)
    {
        var lookup = new Dictionary<string, List<int>>(StringComparer.Ordinal);

        for (var index = 0; index < rows.Count; index++)
        {
            if (DataPipelineKeys.HasEmptyKey(rows[index], keys))
            {
                continue;
            }

            var key = DataPipelineKeys.Build(rows[index], keys, ignoreCase);

            if (!lookup.TryGetValue(key, out var list))
            {
                list = [];
                lookup[key] = list;
            }

            list.Add(index);
        }

        return lookup;
    }

    private static object[] Combine(object[] left, object[] right, Plan plan)
    {
        var row = new object[plan.Fields.Length];

        if (left is not null)
        {
            Array.Copy(left, row, Math.Min(left.Length, plan.LeftCount));
        }

        if (right is not null)
        {
            Array.Copy(right, 0, row, plan.LeftCount, Math.Min(right.Length, plan.Fields.Length - plan.LeftCount));
        }

        return row;
    }

    private Plan BuildPlan(IReadOnlyList<DataField> left, IReadOnlyList<DataField> right, JoinStepSettings settings, out List<string> errors)
    {
        errors = [];

        var leftKeys = new List<int>();
        var rightKeys = new List<int>();

        if (settings.Keys.Count == 0)
        {
            errors.Add(S["Choose the fields that match the rows of both inputs."]);
        }

        foreach (var key in settings.Keys)
        {
            var leftIndex = left.IndexOf(key.LeftField);
            var rightIndex = right.IndexOf(key.RightField);

            if (leftIndex < 0)
            {
                errors.Add(S["The left input has no field named '{0}'.", key.LeftField ?? string.Empty]);
            }

            if (rightIndex < 0)
            {
                errors.Add(S["The right input has no field named '{0}'.", key.RightField ?? string.Empty]);
            }

            if (leftIndex >= 0 && rightIndex >= 0)
            {
                leftKeys.Add(leftIndex);
                rightKeys.Add(rightIndex);
            }
        }

        var prefix = string.IsNullOrWhiteSpace(settings.RightPrefix) ? "Right." : settings.RightPrefix;
        var names = new HashSet<string>(left.Select(field => field.Name), StringComparer.Ordinal);
        var fields = left.ToList();

        foreach (var field in right)
        {
            var name = field.Name;

            while (!names.Add(name))
            {
                name = prefix + name;
            }

            fields.Add(name == field.Name ? field : field.WithName(name, prefix + field.DisplayName));
        }

        return new Plan(fields.ToArray(), left.Count, [.. leftKeys], [.. rightKeys]);
    }

    private sealed record Plan(DataField[] Fields, int LeftCount, int[] LeftKeys, int[] RightKeys);
}
