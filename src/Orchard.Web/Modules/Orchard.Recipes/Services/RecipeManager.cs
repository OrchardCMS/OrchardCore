using System;
using System.Threading.Tasks;
using Orchard.Recipes.Models;
using Orchard.Environment.Shell.State;
using Microsoft.Extensions.Logging;
using Orchard.Recipes.Events;
using Orchard.Events;
using YesSql.Core.Services;
using Orchard.FileSystem;

namespace Orchard.Recipes.Services
{
    public class RecipeManager : IRecipeManager
    {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IRecipeScheduler _recipeScheduler;
        private readonly IRecipeParser _recipeParser;
        private readonly IOrchardFileSystem _fileSystem;
        private readonly IEventBus _eventBus;
        private readonly ISession _session;
        private readonly ILogger _logger;

        private readonly ContextState<string> _executionIds = new ContextState<string>("executionid");

        public RecipeManager(
            IRecipeStepQueue recipeStepQueue,
            IRecipeScheduler recipeScheduler,
            IRecipeParser recipeParser,
            IOrchardFileSystem fileSystem,
            IEventBus eventBus,
            ISession session,
            ILogger<RecipeManager> logger)
        {
            _recipeStepQueue = recipeStepQueue;
            _recipeScheduler = recipeScheduler;
            _recipeParser = recipeParser;
            _fileSystem = fileSystem;
            _eventBus = eventBus;
            _session = session;
            _logger = logger;
        }

        public Task<string> ExecuteAsync(RecipeDescriptor recipeDescriptor)
        {
            var executionId = Guid.NewGuid().ToString("n");

            _executionIds.SetState(executionId);

            _logger.LogInformation("Executing recipe '{0}'.", recipeDescriptor.Name);
            try
            {
                _eventBus.NotifyAsync<IRecipeExecuteEventHandler>(x => 
                    x.ExecutionStartAsync(executionId, recipeDescriptor)).Wait();
                
                _recipeParser.ProcessRecipe(
                    _fileSystem.GetFileInfo(recipeDescriptor.Location), (recipe, recipeStep) =>
                {
                    ExecuteRecipeStep(executionId, recipe, recipeStep);
                });

                //await _recipeScheduler.ScheduleWork(executionId);
                return null;
                //return executionId;
            }
            finally
            {
                _executionIds.SetState(null);
            }
        }

        public void ExecuteRecipeStep(string executionId, RecipeDescriptor recipe, RecipeStepDescriptor recipeStep)
        {
            _recipeStepQueue.Enqueue(executionId, recipeStep);
            _session.Save(new RecipeStepResult
            {
                RecipeName = recipe.Name,
                StepName = recipeStep.Name
            });
        }
    }
}
