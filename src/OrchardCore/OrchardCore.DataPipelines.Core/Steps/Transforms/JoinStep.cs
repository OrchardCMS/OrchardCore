using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataSources.Operations;
using OrchardCore.DataSources;
using OrchardCore.Localization;
using static OrchardCore.Localization.LocalizationMetadata;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Combines each row of the left input with the rows of the right input whose key fields hold the same values, like a
/// SQL join, with <see cref="DataJoin"/>. The step reads the right input first: when it fits in
/// <see cref="DataPipelineOptions.StepMemoryLimitMegabytes"/>, the left rows stream through it, so connect the smaller
/// data set to the right; otherwise both inputs are sorted in temporary files and merged. A field of the right input
/// whose name the left input already has gets a prefix.
/// </summary>
public sealed class JoinStep : DataPipelineStepType<JoinStep, JoinStepSettings>, IDataPipelineStepType
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

    public static string Name => StepName;

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<JoinStep>("Join");

    public static LocalizationSource Description { get; } = LocalizationSource.Create<JoinStep>("Combines the rows of two inputs that share key values.");

    public static DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

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

        await using var right = await DataPipelineRowReader.OpenAsync(context.GetInput(Right), context.CancellationToken);
        await using var left = await DataPipelineRowReader.OpenAsync(context.GetInput(Left), context.CancellationToken);

        var plan = BuildPlan(left.Fields, right.Fields, settings, out var errors);

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(errors[0]);
        }

        var joined = DataJoin.JoinAsync(
            left.ReadRowsAsync(),
            right.ReadRowsAsync(),
            plan.LeftCount,
            plan.Fields.Length - plan.LeftCount,
            plan.LeftKeys,
            plan.RightKeys,
            settings.JoinType,
            settings.IgnoreCase,
            DataPipelineRowReader.CreateSpillOptions(context),
            context.CancellationToken);

        await DataPipelineRowReader.WriteRowsAsync(context.GetOutput(), plan.Fields, joined, context.CancellationToken);
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
                errors.Add(S["The left input has no field named '{0}'.", Arg("field name", key.LeftField ?? string.Empty)]);
            }

            if (rightIndex < 0)
            {
                errors.Add(S["The right input has no field named '{0}'.", Arg("field name", key.RightField ?? string.Empty)]);
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
