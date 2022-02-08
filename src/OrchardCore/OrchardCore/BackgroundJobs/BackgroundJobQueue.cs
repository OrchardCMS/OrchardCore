using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundJobs
{
    public class BackgroundJobQueue : IBackgroundJobQueue
    {
        private readonly Channel<BackgroundJobQueueItem> _queue;

        public string Name => "Default";

        public BackgroundJobQueue()
        // TODO capacity
        {
            // Capacity should be set based on the expected application load and
            // number of concurrent threads accessing the queue.            
            // BoundedChannelFullMode.Wait will cause calls to WriteAsync() to return a task,
            // which completes only when space became available. This leads to backpressure,
            // in case too many publishers/calls start accumulating.
            var options = new BoundedChannelOptions(5)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<BackgroundJobQueueItem>(options);
        }

        public ValueTask QueueBackgroundJobAsync(BackgroundJobQueueItem workItem)
            => _queue.Writer.WriteAsync(workItem);

        public ValueTask<BackgroundJobQueueItem> DequeueAsync(CancellationToken cancellationToken)
            => _queue.Reader.ReadAsync(cancellationToken);

        public IAsyncEnumerable<BackgroundJobQueueItem> DequeueAllAsync(CancellationToken cancellationToken)
            => _queue.Reader.ReadAllAsync(cancellationToken);
    }
}
