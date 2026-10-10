using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Expressions;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Adds fields computed by formulas, such as <c>[Price] * [Quantity]</c> or <c>UPPER([Country])</c>. A formula can
/// use the fields calculated before it, and a calculated field with the name of an existing field replaces its value.
/// </summary>
public sealed class CalculatedFieldsStep : DataPipelineStepType<CalculatedFieldsStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "CalculatedFields";

    private readonly IStringLocalizer S;

    public CalculatedFieldsStep(IStringLocalizer<CalculatedFieldsStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Add calculated fields"];

    public override LocalizedString Description => S["Adds fields computed by formulas."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

    public override string Icon => "fa-solid fa-calculator";

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);

        if (settings.Fields.Count == 0)
        {
            context.AddWarning(S["Add a calculated field."]);
        }

        var plan = BuildPlan(context.GetInputFields(), settings, out var errors);

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
        var output = context.GetOutput();
        var now = await DataPipelineFormulas.GetRunTimeAsync(context.Run);
        var warnings = 0;

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

            var rows = new List<object[]>(batch.Count);

            foreach (var source in batch.Rows)
            {
                var row = new object[plan.Fields.Count];
                Array.Copy(source, row, Math.Min(source.Length, batch.Fields.Count));

                foreach (var calculation in plan.Calculations)
                {
                    var value = DataPipelineFormulas.Evaluate(calculation.Expression, row, now, out var error);

                    if (error is not null && warnings++ < 10)
                    {
                        context.LogWarning(S["The field '{0}' couldn't be calculated for a row: {1}", calculation.Name, error]);
                    }

                    row[calculation.Index] = DataValues.Coerce(value, plan.Fields[calculation.Index].Type);
                }

                rows.Add(row);
            }

            await output.WriteAsync(new DataBatch(plan.Fields, rows), context.CancellationToken);
        }
    }

    private Plan BuildPlan(IReadOnlyList<DataField> input, CalculatedFieldsStepSettings settings, out List<string> errors)
    {
        errors = [];

        var fields = input.ToList();
        var calculations = new List<Calculation>();

        foreach (var field in settings.Fields)
        {
            var name = field.Name?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                errors.Add(S["Enter the name of each calculated field."]);

                continue;
            }

            var expression = DataPipelineFormulas.TryCompile(field.Formula, fields, S, out var error);

            if (expression is null)
            {
                errors.Add(S["The formula of '{0}' is invalid: {1}", name, error]);

                continue;
            }

            var dataField = new DataField(name, name, expression.DataType, S["Calculated"]);
            var index = fields.IndexOf(name);

            if (index < 0)
            {
                index = fields.Count;
                fields.Add(dataField);
            }
            else
            {
                fields[index] = dataField;
            }

            calculations.Add(new Calculation(name, index, expression));
        }

        return new Plan(fields, calculations);
    }

    private sealed record Calculation(string Name, int Index, CompiledExpression Expression);

    private sealed record Plan(List<DataField> Fields, List<Calculation> Calculations);
}
