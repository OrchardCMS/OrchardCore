using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// An input of a running step.
/// </summary>
public abstract class DataPipelineInput
{
    /// <summary>
    /// Gets the port of the input.
    /// </summary>
    public abstract DataPipelinePort Port { get; }

    /// <summary>
    /// Gets a value indicating whether the input is connected.
    /// </summary>
    public abstract bool IsConnected { get; }

    /// <summary>
    /// Gets the fields of the rows of a record input, as described before the run. When the input has several
    /// connections, they are the fields of the first one; see <see cref="FieldSets"/>.
    /// </summary>
    public abstract IReadOnlyList<DataField> Fields { get; }

    /// <summary>
    /// Gets the fields of each connection of a record input, in the order of the connections.
    /// </summary>
    public abstract IReadOnlyList<IReadOnlyList<DataField>> FieldSets { get; }

    /// <summary>
    /// Reads the batches of rows of a record input until the steps upstream are done. An input with several
    /// connections reads them one after the other.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The batches.</returns>
    public abstract IAsyncEnumerable<DataBatch> ReadBatchesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the files of a file input until the steps upstream are done.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The files.</returns>
    public abstract IAsyncEnumerable<DataPipelineFile> ReadFilesAsync(CancellationToken cancellationToken = default);
}
