using System;
using System.Threading;
using Orchard.BackgroundTasks;

namespace Orchard.Demo.Services
{
    [BackgroundTask(Group = "G1")]
    public class TestBackgroundTask : IBackgroundTask
    {
        private int _count;

        public void DoWork(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            _count++;
        }
    }

    public class TestBackgroundTaskGroup : IBackgroundTaskGroup
    {
        public string Name()
        {
            return "G1";
        }

        public TimeSpan Period()
        {
            return TimeSpan.FromMinutes(2);
        }
    }
}
