using System.Threading.Tasks;
using Orchard.Environment.Shell;

namespace Orchard.BackgroundTasks
{
    public class BackgroundTasksStarter : IOrchardShellEvents
    {
        private readonly IBackgroundTaskService _backgroundService;

        public BackgroundTasksStarter(IBackgroundTaskService backgroundService)
        {
            _backgroundService = backgroundService;
        }

        public Task ActivatedAsync()
        {
            _backgroundService.Activate();

            return Task.CompletedTask;
        }

        public Task ActivatingAsync()
        {
            return Task.CompletedTask;
        }

        public Task TerminatedAsync()
        {
            return Task.CompletedTask;
        }

        public Task TerminatingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
