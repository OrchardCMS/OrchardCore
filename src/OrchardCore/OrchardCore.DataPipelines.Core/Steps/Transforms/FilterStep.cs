using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Expressions;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Keeps the rows that match a condition, such as <c>[Amount] &gt;= 100 AND [Country] = 'CA'</c>. The rows that
/// don't match go to a second output, which may be left unconnected.
/// </summary>
public sealed class FilterStep : DataPipelineStepType<FilterStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "Filter";

    /// <summary>
    /// The output of the rows that match the condition.
    /// </summary>
    public const string Matched = "Matched";

    /// <summary>
    /// The output of the rows that don't match the condition.
    /// </summary>
    public const string Unmatched = "Unmatched";

    private readonly IStringLocalizer S;

    public FilterStep(IStringLocalizer<FilterStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Filter rows"];

    public override LocalizedString Description => S["Keeps the rows that match a condition."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

    public override string Icon => "fa-solid fa-filter";

    public override IReadOnlyList<DataPipelinePort> GetOutputs(DataPipelineStep step)
        =>
        [
            new(Matched, S["Matched"]),
            new(Unmatched, S["Unmatched"]),
        ];

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var fields = context.GetInputFields();
        var settings = GetSettings(context.Step);

        context.SetOutputFields(fields, Matched);
        context.SetOutputFields(fields, Unmatched);

        if (string.IsNullOrWhiteSpace(settings.Condition))
        {
            context.AddError(S["Enter the condition the rows must match."]);

            return Task.CompletedTask;
        }

        var expression = DataPipelineFormulas.TryCompile(settings.Condition, fields, S, out var error);

        if (expression is null)
        {
            context.AddError(S["The condition is invalid: {0}", error]);
        }
        else if (expression.DataType != DataFieldType.Boolean)
        {
            context.AddError(S["The condition must be true or false, such as [Amount] > 100."]);
        }

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var input = context.GetInput();
        var matched = context.GetOutput(Matched);
        var unmatched = context.GetOutput(Unmatched);
        var settings = GetSettings(context.Step);
        var now = await DataPipelineFormulas.GetNowAsync(context.Services);
        var warnings = 0;

        CompiledExpression expression = null;
        IReadOnlyList<DataField> compiledFor = null;

        await foreach (var batch in input.ReadBatchesAsync(context.CancellationToken))
        {
            if (!ReferenceEquals(compiledFor, batch.Fields))
            {
                expression = DataPipelineFormulas.TryCompile(settings.Condition, batch.Fields, S, out var compileError)
                    ?? throw new InvalidOperationException(S["The condition is invalid: {0}", compileError]);
                compiledFor = batch.Fields;
            }

            var kept = new List<object[]>();
            var dropped = new List<object[]>();

            foreach (var row in batch.Rows)
            {
                var value = DataPipelineFormulas.Evaluate(expression, row, now, out var error);

                if (error is not null && warnings++ < 10)
                {
                    context.LogWarning(S["A row couldn't be checked, and doesn't match: {0}", error]);
                }

                if (value is true)
                {
                    kept.Add(row);
                }
                else
                {
                    dropped.Add(row);
                }
            }

            if (kept.Count > 0)
            {
                await matched.WriteAsync(new DataBatch(batch.Fields, kept), context.CancellationToken);
            }

            if (dropped.Count > 0 && unmatched.IsConnected)
            {
                await unmatched.WriteAsync(new DataBatch(batch.Fields, dropped), context.CancellationToken);
            }
        }
    }
}
