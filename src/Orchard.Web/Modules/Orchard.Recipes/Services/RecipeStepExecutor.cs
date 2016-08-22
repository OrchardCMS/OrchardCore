using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Events;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Recipes.Services
{
    public class RecipeStepExecutor : IRecipeStepExecutor
    {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IEnumerable<IRecipeHandler> _recipeHandlers;
        private readonly IEventBus _eventBus;
        private readonly ISession _session;
        private readonly ILogger _logger;

        public RecipeStepExecutor(
            IRecipeStepQueue recipeStepQueue,
            IEnumerable<IRecipeHandler> recipeHandlers,
            IEventBus eventBus,
            ISession session,
            ILogger<RecipeStepExecutor> logger,
            IStringLocalizer<RecipeStepExecutor> localizer)
        {
            _recipeStepQueue = recipeStepQueue;
            _recipeHandlers = recipeHandlers;
            _eventBus = eventBus;
            _session = session;
            _logger = logger;

            T = localizer;
        }

        public IStringLocalizer T { get; }

        public async Task<bool> ExecuteNextStepAsync(string executionId)
        {
            var nextRecipeStep = await _recipeStepQueue.DequeueAsync(executionId);
            if (nextRecipeStep == null)
            {
                _logger.LogInformation("No more recipe steps left to execute.");

                return false;
            }

            _logger.LogInformation("Executing recipe step '{0}'.", nextRecipeStep.Name);

            var recipeContext = new RecipeContext
            {
                RecipeStep = nextRecipeStep,
                Executed = false,
                ExecutionId = executionId
            };

            _eventBus.Notify<IRecipeExecuteEventHandler>(e =>
                e.RecipeStepExecutingAsync(executionId, recipeContext));

            _recipeHandlers.Invoke(rh => rh.ExecuteRecipeStep(recipeContext), _logger);

            await UpdateStepResultRecordAsync(recipeContext);

            _eventBus.Notify<IRecipeExecuteEventHandler>(e =>
                e.RecipeStepExecutedAsync(executionId, recipeContext));

            return true;
        }

        private async Task UpdateStepResultRecordAsync(RecipeContext recipeContext)
        {
            var stepResults = await _session
                .QueryAsync<RecipeStepResult>()
                .List();

            var stepResultRecord = stepResults
                .First(record => 
                    record.ExecutionId == recipeContext.ExecutionId &&
                    record.StepId == recipeContext.RecipeStep.Id);

            stepResultRecord.IsCompleted = true;
            //stepResultRecord.IsSuccessful = true;
            //stepResultRecord.ErrorMessage = string.Empty;

            _session.Save(stepResultRecord);
            await _session.CommitAsync();
        }
    }
}