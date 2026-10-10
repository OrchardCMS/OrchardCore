using System.Runtime.CompilerServices;
using System.Threading.Channels;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// An input that reads the edges connected to it, one after the other.
/// </summary>
internal sealed class DataPipelineChannelInput : DataPipelineInput
{
    private readonly IReadOnlyList<DataPipelineEdge> _edges;
    private readonly DataPipelineStepMetrics _metrics;

    public DataPipelineChannelInput(
        DataPipelinePort port,
        IReadOnlyList<IReadOnlyList<DataField>> fieldSets,
        IReadOnlyList<DataPipelineEdge> edges,
        DataPipelineStepMetrics metrics)
    {
        Port = port;
        FieldSets = fieldSets ?? [];
        _edges = edges ?? [];
        _metrics = metrics;
    }

    public override DataPipelinePort Port { get; }

    public override bool IsConnected => _edges.Count > 0;

    public override IReadOnlyList<DataField> Fields => FieldSets.Count > 0 ? FieldSets[0] : [];

    public override IReadOnlyList<IReadOnlyList<DataField>> FieldSets { get; }

    public override async IAsyncEnumerable<DataBatch> ReadBatchesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in ReadAsync(cancellationToken))
        {
            if (item is DataBatch batch)
            {
                _metrics.AddRowsIn(batch.Count);

                yield return batch;
            }
        }
    }

    public override async IAsyncEnumerable<DataPipelineFile> ReadFilesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in ReadAsync(cancellationToken))
        {
            if (item is DataPipelineFile file)
            {
                _metrics.AddFileIn();

                yield return file;
            }
        }
    }

    public void Complete()
    {
        foreach (var edge in _edges)
        {
            edge.CompleteConsumer();
        }
    }

    private async IAsyncEnumerable<object> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var edge in _edges)
        {
            var reader = edge.Channel.Reader;

            while (true)
            {
                bool more;

                try
                {
                    more = await reader.WaitToReadAsync(cancellationToken);
                }
                catch (ChannelClosedException)
                {
                    // The step upstream failed or was cancelled: what this step read so far is incomplete.
                    throw new OperationCanceledException(cancellationToken);
                }

                if (!more)
                {
                    break;
                }

                while (reader.TryRead(out var item))
                {
                    yield return item;
                }
            }
        }

        // A step must not deliver what it read when the run was cancelled meanwhile.
        cancellationToken.ThrowIfCancellationRequested();
    }
}

/// <summary>
/// An output that copies what is written to every edge connected to it.
/// </summary>
internal sealed class DataPipelineChannelOutput : DataPipelineOutput
{
    private readonly IReadOnlyList<DataPipelineEdge> _edges;
    private readonly DataPipelineStepMetrics _metrics;

    public DataPipelineChannelOutput(
        DataPipelinePort port,
        IReadOnlyList<DataField> fields,
        IReadOnlyList<DataPipelineEdge> edges,
        DataPipelineStepMetrics metrics)
    {
        Port = port;
        Fields = fields ?? [];
        _edges = edges ?? [];
        _metrics = metrics;
    }

    public override DataPipelinePort Port { get; }

    public override bool IsConnected => _edges.Count > 0;

    public override bool IsClosed => _edges.Count > 0 && _edges.All(edge => edge.ConsumerDone);

    public override IReadOnlyList<DataField> Fields { get; }

    public override async ValueTask WriteAsync(DataBatch batch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);

        if (Port.Kind != DataPipelinePortKind.Records)
        {
            throw new InvalidOperationException($"The output '{Port.Name}' carries files, not rows.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        _metrics.AddRowsOut(batch.Count);

        foreach (var edge in _edges)
        {
            await edge.WriteAsync(batch, cancellationToken);
        }
    }

    public override async ValueTask WriteFileAsync(DataPipelineFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (Port.Kind != DataPipelinePortKind.Files)
        {
            throw new InvalidOperationException($"The output '{Port.Name}' carries rows, not files.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        _metrics.AddFileOut();

        foreach (var edge in _edges)
        {
            await edge.WriteAsync(file, cancellationToken);
        }
    }

    public void Complete(Exception error = null)
    {
        foreach (var edge in _edges)
        {
            edge.CompleteProducer(error);
        }
    }
}
