using System.Threading.Channels;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// The channel that carries batches or files along one connection while a pipeline runs.
/// </summary>
internal sealed class DataPipelineEdge
{
    // A few batches in flight let steps work in parallel, while a slow step makes the faster ones wait instead of
    // filling the memory.
    private const int BoundedCapacity = 4;

    private volatile bool _consumerDone;

    public DataPipelineEdge(bool unbounded)
    {
        Channel = unbounded
            ? System.Threading.Channels.Channel.CreateUnbounded<object>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true })
            : System.Threading.Channels.Channel.CreateBounded<object>(new BoundedChannelOptions(BoundedCapacity)
            {
                SingleReader = true,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait,
            });
    }

    public Channel<object> Channel { get; }

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

        try
        {
            await Channel.Writer.WriteAsync(item, cancellationToken);
        }
        catch (ChannelClosedException)
        {
            // The reader is gone; nothing to deliver.
        }
    }

    /// <summary>
    /// Marks the reading step as done, and drops what is waiting, which releases a writer waiting for room.
    /// </summary>
    public void CompleteConsumer()
    {
        _consumerDone = true;

        while (Channel.Reader.TryRead(out _))
        {
        }
    }

    public void CompleteProducer(Exception error = null)
        => Channel.Writer.TryComplete(error);
}
