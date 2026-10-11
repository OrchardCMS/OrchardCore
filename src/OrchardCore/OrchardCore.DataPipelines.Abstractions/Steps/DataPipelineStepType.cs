using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;
using OrchardCore.Entities;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// A base class for step types. Its ports follow the category: a source has one record output; a transform has one
/// required record input and one record output; a file step has one required record input and one file output; a
/// destination has one required file input. By default a step passes the fields of its input to its output.
/// </summary>
public abstract class DataPipelineStepType : IDataPipelineStepType
{
    public abstract string Name { get; }

    public abstract LocalizedString DisplayName { get; }

    public abstract LocalizedString Description { get; }

    public abstract DataPipelineStepCategory Category { get; }

    public virtual string Icon => "fa-solid fa-cube";

    public virtual IReadOnlyList<DataPipelinePort> GetInputs(DataPipelineStep step)
        => Category switch
        {
            DataPipelineStepCategory.Source => [],
            DataPipelineStepCategory.Destination => [new(DataPipelinePort.Input, "Files", DataPipelinePortKind.Files) { IsRequired = true }],
            _ => [new(DataPipelinePort.Input, "Rows") { IsRequired = true }],
        };

    public virtual IReadOnlyList<DataPipelinePort> GetOutputs(DataPipelineStep step)
        => Category switch
        {
            DataPipelineStepCategory.Destination => [],
            DataPipelineStepCategory.File => [new(DataPipelinePort.Output, "Files", DataPipelinePortKind.Files)],
            _ => [new(DataPipelinePort.Output, "Rows")],
        };

    public virtual Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var output in GetOutputs(context.Step))
        {
            if (output.Kind == DataPipelinePortKind.Records)
            {
                context.SetOutputFields(context.GetInputFields(), output.Name);
            }
        }

        return Task.CompletedTask;
    }

    public abstract Task ExecuteAsync(DataPipelineStepContext context);
}

/// <summary>
/// A base class for step types whose settings are stored as one <typeparamref name="TSettings"/> object.
/// </summary>
/// <typeparam name="TSettings">The type of the settings.</typeparam>
public abstract class DataPipelineStepType<TSettings> : DataPipelineStepType
    where TSettings : class, new()
{
    /// <summary>
    /// Reads the settings of a step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns>The settings, or new settings when the step has none yet.</returns>
    public static TSettings GetSettings(DataPipelineStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        return step.GetOrCreate<TSettings>();
    }

    /// <summary>
    /// Writes the settings of a step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <param name="settings">The settings.</param>
    public static void SetSettings(DataPipelineStep step, TSettings settings)
    {
        ArgumentNullException.ThrowIfNull(step);

        step.Put(settings ?? new TSettings());
    }
}
