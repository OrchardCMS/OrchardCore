using System;
using System.Threading;
using Orchard.BackgroundTasks;

namespace Orchard.Demo.Services
{
    public class TestBackgroundTask : IBackgroundTask
    {
        private int _count;

        public void DoWork(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            _count++;
        }
    }
}
