using System.Threading.Tasks;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTasksStarter : IModularTenantEvents
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
            _backgroundService.Terminate();

            return Task.CompletedTask;
        }

        public Task TerminatingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
