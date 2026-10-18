using System.Threading.Channels;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Operations;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// The channel that carries batches or files along one connection while a pipeline runs.
/// </summary>
internal sealed class DataPipelineEdge
{
    // A few batches in flight let steps work in parallel, while a slow step makes the faster ones wait instead of
    // filling the memory.
    private const int BoundedCapacity = 4;

    private readonly Channel<object> _channel;
    private readonly string _spillDirectory;
    private readonly CancellationToken _cancellationToken;
    private readonly object _lock = new();

    // What waits on disk, in order: a batch is its fields here and its rows in the row queue.
    private readonly Queue<object> _spilled = new();
    private DataRowQueue _rows;
    private bool _spilling;
    private bool _pumping;
    private bool _producerDone;
    private Exception _producerError;
    private volatile bool _consumerDone;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataPipelineEdge"/> class.
    /// </summary>
    /// <param name="spillDirectory">When set, a writer never waits for the reader: what doesn't fit in the channel
    /// waits in a temporary file of this directory. A step with several inputs, which may read them one after the
    /// other, needs it so that a step writing to several of them doesn't wait forever.</param>
    /// <param name="cancellationToken">The token of the run.</param>
    public DataPipelineEdge(string spillDirectory = null, CancellationToken cancellationToken = default)
    {
        _spillDirectory = spillDirectory;
        _cancellationToken = cancellationToken;
        _channel = Channel.CreateBounded<object>(new BoundedChannelOptions(BoundedCapacity)
        {
            SingleReader = true,
            SingleWriter = spillDirectory is null,
            FullMode = BoundedChannelFullMode.Wait,
        });
    }

    public ChannelReader<object> Reader => _channel.Reader;

    /// <summary>
    /// Gets a value indicating whether the step reading this edge is done. What is written afterwards is dropped.
    /// </summary>
    public bool ConsumerDone => _consumerDone;

    public async ValueTask WriteAsync(object item, CancellationToken cancellationToken)
    {
        if (_consumerDone)
        {
            return;
        }

        if (_spillDirectory is null)
        {
            try
            {
                await _channel.Writer.WriteAsync(item, cancellationToken);
            }
            catch (ChannelClosedException)
            {
                // The reader is gone; nothing to deliver.
            }

            return;
        }

        lock (_lock)
        {
            // Once something waits on disk, everything after it does too, to keep the order.
            if (!_spilling && _channel.Writer.TryWrite(item))
            {
                return;
            }

            _spilling = true;

            if (item is DataBatch batch)
            {
                (_rows ??= new DataRowQueue(_spillDirectory)).Enqueue(batch.Rows);
                _spilled.Enqueue(new SpilledBatch(batch.Fields));
            }
            else
            {
                _spilled.Enqueue(item);
            }

            if (!_pumping)
            {
                _pumping = true;
                _ = Task.Run(PumpAsync, CancellationToken.None);
            }
        }
    }

    /// <summary>
    /// Marks the reading step as done, and drops what is waiting, which releases a writer waiting for room.
    /// </summary>
    public void CompleteConsumer()
    {
        _consumerDone = true;
        _channel.Writer.TryComplete();

        while (_channel.Reader.TryRead(out _))
        {
        }

        lock (_lock)
        {
            _spilled.Clear();
            _rows?.Dispose();
            _rows = null;
        }
    }

    public void CompleteProducer(Exception error = null)
    {
        lock (_lock)
        {
            _producerDone = true;
            _producerError = error;

            // While items wait on disk, the pump completes the channel after delivering them.
            if (!_spilling)
            {
                _channel.Writer.TryComplete(error);
            }
        }
    }

    // Moves what waits on disk to the channel as the reader makes room.
    private async Task PumpAsync()
    {
        while (true)
        {
            object item;

            lock (_lock)
            {
                if (_consumerDone || _spilled.Count == 0)
                {
                    _spilling = false;
                    _pumping = false;

                    if (_producerDone)
                    {
                        _channel.Writer.TryComplete(_producerError);
                    }

                    return;
                }

                item = _spilled.Dequeue();

                if (item is SpilledBatch spilled)
                {
                    _rows.TryDequeue(out var rows);
                    item = new DataBatch(spilled.Fields, rows);
                }
            }

            try
            {
                await _channel.Writer.WriteAsync(item, _cancellationToken);
            }
            catch (Exception ex) when (ex is ChannelClosedException or OperationCanceledException)
            {
                lock (_lock)
                {
                    _pumping = false;
                }

                return;
            }
        }
    }

    private sealed record SpilledBatch(IReadOnlyList<DataField> Fields);
}
