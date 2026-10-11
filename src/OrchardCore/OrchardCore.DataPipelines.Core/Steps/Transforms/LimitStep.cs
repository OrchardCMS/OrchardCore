using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Keeps a number of rows, optionally after skipping the first ones. Once it has its rows, the step stops reading, and
/// the steps before it stop too.
/// </summary>
public sealed class LimitStep : DataPipelineStepType<LimitStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "Limit";

    private readonly IStringLocalizer S;

    public LimitStep(IStringLocalizer<LimitStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Keep first rows"];

    public override LocalizedString Description => S["Keeps a number of rows, optionally after skipping some."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

    public override string Icon => "fa-solid fa-scissors";

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);

        if (settings.Count <= 0)
        {
            context.AddError(S["Enter the number of rows to keep."]);
        }

        if (settings.Skip < 0)
        {
            context.AddError(S["The number of rows to skip can't be negative."]);
        }

        context.SetOutputFields(context.GetInputFields());

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var skip = (long)Math.Max(0, settings.Skip);
        var remaining = (long)settings.Count;
        var output = context.GetOutput();

        await foreach (var batch in context.GetInput().ReadBatchesAsync(context.CancellationToken))
        {
            var rows = new List<object[]>();

            foreach (var row in batch.Rows)
            {
                if (skip > 0)
                {
                    skip--;

                    continue;
                }

                rows.Add(row);

                if (--remaining == 0)
                {
                    break;
                }
            }

            if (rows.Count > 0)
            {
                await output.WriteAsync(new DataBatch(batch.Fields, rows), context.CancellationToken);
            }

            if (remaining <= 0)
            {
                return;
            }
        }
    }
}
