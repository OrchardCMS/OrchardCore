namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Identifies what flows through a port. A connection joins ports of the same kind.
/// </summary>
public enum DataPipelinePortKind
{
    /// <summary>
    /// Batches of rows with typed fields.
    /// </summary>
    Records,

    /// <summary>
    /// Files, such as a CSV export, ready to be delivered.
    /// </summary>
    Files,
}
