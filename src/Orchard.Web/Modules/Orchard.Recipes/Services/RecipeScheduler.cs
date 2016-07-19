using Microsoft.Extensions.Logging;
using Orchard.DeferredTasks;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.State;
using Orchard.Events;
using Orchard.Recipes.Events;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public class RecipeScheduler : IRecipeScheduler, IRecipeSchedulerEventHandler
    {
        private readonly ShellSettings _shellSettings;
        private readonly IDeferredTaskEngine _processingEngine;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IRecipeStepExecutor _recipeStepExecutor;
        private readonly IEventBus _eventBus;

        private readonly ILogger _logger;

        private readonly ContextState<string> _executionIds = new ContextState<string>("executionid");

        public RecipeScheduler(
            ShellSettings shellSettings,
            IDeferredTaskEngine processingEngine,
            IShellDescriptorManager shellDescriptorManager,
            IRecipeStepExecutor recipeStepExecutor,
            IEventBus eventBus,
            ILogger<RecipeScheduler> logger)
        {
            _shellSettings = shellSettings;
            _processingEngine = processingEngine;
            _shellDescriptorManager = shellDescriptorManager;
            _recipeStepExecutor = recipeStepExecutor;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task ExecuteWorkAsync(string executionId)
        {
            _executionIds.SetState(executionId);
            try
            {
                // todo: this callback should be guarded against concurrency by the IProcessingEngine
                var scheduleMore = _recipeStepExecutor.ExecuteNextStep(executionId);
                if (scheduleMore)
                {
                    _logger.LogInformation("Scheduling next step of recipe.");
                    await ScheduleWorkAsync(executionId);
                }
                else
                {
                    _logger.LogInformation("All recipe steps executed; restarting shell.");

                    // Because recipes execute in their own workcontext, we need to restart the shell, as signaling a cache won't work across workcontexts.
                    var shellDescriptor = await _shellDescriptorManager.GetShellDescriptorAsync();
                    _eventBus.NotifyAsync<IShellDescriptorManagerEventHandler>(x =>
                        x.Changed(shellDescriptor, _shellSettings.Name)).Wait();
                }
            }
            finally
            {
                _executionIds.SetState(null);
            }
        }

        public async Task ScheduleWorkAsync(string executionId)
        {
            var shellDescriptor = await _shellDescriptorManager.GetShellDescriptorAsync();

            _processingEngine.AddTask(async x => {
                await ExecuteWorkAsync(executionId);
            });
        }
    }
}
