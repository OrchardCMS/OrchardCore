using System;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Demo.Services
{
    public class TestBackgroundTask : IBackgroundTask
    {
#pragma warning disable IDE0052 // Remove unread private members
        private int _count;
#pragma warning restore IDE0052 // Remove unread private members

        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            _count++;

            return Task.CompletedTask;
        }
    }
}
