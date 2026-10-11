using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// A kind of step that data pipelines are built from, such as reading a data source, filtering rows, creating a file
/// or saving it to the media library. A step type is a stateless service: the settings of each step of a pipeline are
/// stored on its <see cref="DataPipelineStep"/>. Register an implementation with
/// <c>services.AddDataPipelineStep&lt;TStepType, TDisplayDriver&gt;()</c>; its display driver renders the step's
/// editor in the designer.
/// </summary>
public interface IDataPipelineStepType
{
    /// <summary>
    /// Gets the stable technical name of the step type. Pipelines store this name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the label shown in the designer.
    /// </summary>
    LocalizedString DisplayName { get; }

    /// <summary>
    /// Gets the description shown in the designer's toolbox.
    /// </summary>
    LocalizedString Description { get; }

    /// <summary>
    /// Gets the category the step is listed under, which also tells whether the step has effects outside the run.
    /// </summary>
    DataPipelineStepCategory Category { get; }

    /// <summary>
    /// Gets the Font Awesome classes of the step's icon, such as <c>fa-solid fa-filter</c>.
    /// </summary>
    string Icon { get; }

    /// <summary>
    /// Lists the inputs of a step.
    /// </summary>
    /// <param name="step">The step, whose settings may change its inputs.</param>
    /// <returns>The inputs, in the order the designer shows them.</returns>
    IReadOnlyList<DataPipelinePort> GetInputs(DataPipelineStep step);

    /// <summary>
    /// Lists the outputs of a step.
    /// </summary>
    /// <param name="step">The step, whose settings may change its outputs.</param>
    /// <returns>The outputs, in the order the designer shows them.</returns>
    IReadOnlyList<DataPipelinePort> GetOutputs(DataPipelineStep step);

    /// <summary>
    /// Checks the settings of a step against the fields of its inputs, and describes the fields of each of its record
    /// outputs. The designer calls it to list the fields available to the next steps and to report issues, and the
    /// engine calls it before a run.
    /// </summary>
    /// <param name="context">The step, the fields of its inputs, and where to put its output fields and issues.</param>
    /// <returns>A task that completes when the step is described.</returns>
    Task DescribeAsync(DataPipelineDescribeContext context);

    /// <summary>
    /// Runs a step: reads its inputs and writes its outputs until its inputs are exhausted.
    /// </summary>
    /// <param name="context">The step, its inputs and outputs, and the run it belongs to.</param>
    /// <returns>A task that completes when the step is done.</returns>
    Task ExecuteAsync(DataPipelineStepContext context);
}
