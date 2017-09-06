using System;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Demo.Services
{
    public class TestBackgroundTask : IBackgroundTask
    {
        private int _count;

        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            _count++;

            return Task.CompletedTask;
        }
    }
}
