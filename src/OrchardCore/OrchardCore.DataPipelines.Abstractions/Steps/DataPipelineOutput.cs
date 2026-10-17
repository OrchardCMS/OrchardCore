using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// An output of a running step.
/// </summary>
public abstract class DataPipelineOutput
{
    /// <summary>
    /// Gets the port of the output.
    /// </summary>
    public abstract DataPipelinePort Port { get; }

    /// <summary>
    /// Gets a value indicating whether the output is connected. Whatever is written to an output that is not
    /// connected is dropped.
    /// </summary>
    public abstract bool IsConnected { get; }

    /// <summary>
    /// Gets a value indicating whether every step connected to the output is done reading, such as a step that keeps
    /// the first rows only. A step can then stop producing rows early.
    /// </summary>
    public abstract bool IsClosed { get; }

    /// <summary>
    /// Gets the fields of the rows of a record output, as described before the run.
    /// </summary>
    public abstract IReadOnlyList<DataField> Fields { get; }

    /// <summary>
    /// Writes a batch of rows to a record output. It waits while the steps downstream are busy, so a fast step
    /// doesn't fill the memory.
    /// </summary>
    /// <param name="batch">The batch.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the batch is accepted.</returns>
    public abstract ValueTask WriteAsync(DataBatch batch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a file to a file output.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the file is accepted.</returns>
    public abstract ValueTask WriteFileAsync(DataPipelineFile file, CancellationToken cancellationToken = default);
}
