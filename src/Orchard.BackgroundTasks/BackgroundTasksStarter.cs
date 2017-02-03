using System.Threading.Tasks;
using Microsoft.AspNetCore.Modules;

namespace Orchard.BackgroundTasks
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
            return Task.CompletedTask;
        }

        public Task TerminatingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
