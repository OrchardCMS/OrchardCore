using OrchardCore.DataPipelines.Models;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Gives the editors of steps the context of the step they edit, such as the fields of its inputs, so they can offer
/// them in pickers. The designer sets it before it renders or applies an editor. Registered as a scoped service.
/// </summary>
public sealed class DataPipelineEditorContext
{
    /// <summary>
    /// Gets or sets the pipeline the step belongs to.
    /// </summary>
    public DataPipeline Pipeline { get; set; }

    /// <summary>
    /// Gets or sets the analysis of the pipeline's draft.
    /// </summary>
    public DataPipelineAnalysis Analysis { get; set; }

    /// <summary>
    /// Gets or sets the step being edited.
    /// </summary>
    public DataPipelineStep Step { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the designer must render the editor again after it is applied, such as
    /// when choosing a data set changes the fields the editor offers.
    /// </summary>
    public bool ReloadEditor { get; set; }

    /// <summary>
    /// Gets the fields of an input of the step being edited.
    /// </summary>
    /// <param name="port">The input name.</param>
    /// <returns>The fields, or an empty list.</returns>
    public IReadOnlyList<DataField> GetInputFields(string port = Steps.DataPipelinePort.Input)
        => Analysis?.GetStep(Step?.StepId)?.GetInputFields(port) ?? [];
}
