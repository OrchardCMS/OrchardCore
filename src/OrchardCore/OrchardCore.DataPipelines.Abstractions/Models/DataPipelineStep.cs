using OrchardCore.Entities;

namespace OrchardCore.DataPipelines.Models;

/// <summary>
/// One step of a data pipeline, such as a data source, a filter or a destination. Its settings are stored in
/// <see cref="Entity.Properties"/>; read them with <c>step.GetOrCreate&lt;TSettings&gt;()</c> and write them with
/// <c>step.Put(settings)</c>.
/// </summary>
public sealed class DataPipelineStep : Entity
{
    /// <summary>
    /// Gets or sets the identifier of the step, unique within its pipeline.
    /// </summary>
    public string StepId { get; set; }

    /// <summary>
    /// Gets or sets the name of the step type, see <see cref="Steps.IDataPipelineStepType.Name"/>.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the optional title the designer shows instead of the step type's name.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the horizontal position of the step on the designer canvas.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the vertical position of the step on the designer canvas.
    /// </summary>
    public int Y { get; set; }
}
