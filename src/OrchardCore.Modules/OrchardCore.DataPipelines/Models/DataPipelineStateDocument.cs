using System.Text.Json.Nodes;

namespace OrchardCore.DataPipelines.Models;

/// <summary>
/// The values a pipeline keeps from one run to the next, saved by its last successful run. See
/// <see cref="Steps.DataPipelineState"/>.
/// </summary>
public sealed class DataPipelineStateDocument
{
    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the pipeline.
    /// </summary>
    public string PipelineId { get; set; }

    /// <summary>
    /// Gets or sets the values, by key.
    /// </summary>
    public Dictionary<string, JsonNode> Values { get; set; } = [];

    /// <summary>
    /// Gets or sets when the values were last saved.
    /// </summary>
    public DateTime ModifiedUtc { get; set; }

    /// <summary>
    /// Gets or sets the run that last saved the values.
    /// </summary>
    public string ModifiedByRunId { get; set; }
}
