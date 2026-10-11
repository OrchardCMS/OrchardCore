using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Keeps, orders, renames and converts the fields of the rows. Without any field selected, the rows pass unchanged.
/// </summary>
public sealed class SelectFieldsStep : DataPipelineStepType<SelectFieldsStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "SelectFields";

    private readonly IStringLocalizer S;

    public SelectFieldsStep(IStringLocalizer<SelectFieldsStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Select fields"];

    public override LocalizedString Description => S["Keeps, orders, renames and converts fields."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

    public override string Icon => "fa-solid fa-table-columns";

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var plan = BuildPlan(context.GetInputFields(), GetSettings(context.Step), out var errors);

        foreach (var error in errors)
        {
            context.AddError(error);
        }

        context.SetOutputFields(plan.Select(column => column.Field).ToArray());

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var output = context.GetOutput();

        List<Column> plan = null;
        DataField[] fields = null;
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

                fields = plan.Select(column => column.Field).ToArray();
                plannedFor = batch.Fields;
            }

            var rows = new List<object[]>(batch.Count);

            foreach (var source in batch.Rows)
            {
                var row = new object[plan.Count];

                for (var index = 0; index < plan.Count; index++)
                {
                    var value = plan[index].SourceIndex < source.Length ? source[plan[index].SourceIndex] : null;
                    row[index] = plan[index].Convert ? DataValues.Coerce(value, plan[index].Field.Type) : value;
                }

                rows.Add(row);
            }

            await output.WriteAsync(new DataBatch(fields, rows), context.CancellationToken);
        }
    }

    private List<Column> BuildPlan(IReadOnlyList<DataField> input, SelectFieldsStepSettings settings, out List<string> errors)
    {
        errors = [];

        if (settings.Fields.Count == 0)
        {
            return input.Select((field, index) => new Column(field, index, false)).ToList();
        }

        var plan = new List<Column>();
        var names = new HashSet<string>(StringComparer.Ordinal);

        foreach (var selected in settings.Fields)
        {
            var index = input.IndexOf(selected.Field);

            if (index < 0)
            {
                errors.Add(S["The input has no field named '{0}'.", selected.Field ?? string.Empty]);

                continue;
            }

            var source = input[index];
            var name = string.IsNullOrWhiteSpace(selected.Name) ? source.Name : selected.Name.Trim();

            if (!names.Add(name))
            {
                errors.Add(S["Two selected fields are named '{0}'.", name]);

                continue;
            }

            var type = selected.Type ?? source.Type;
            var field = name == source.Name && type == source.Type
                ? source
                : new DataField(name, name == source.Name ? source.DisplayName : name, type, source.Group) { Description = source.Description };

            plan.Add(new Column(field, index, type != source.Type));
        }

        return plan;
    }

    private sealed record Column(DataField Field, int SourceIndex, bool Convert);
}
