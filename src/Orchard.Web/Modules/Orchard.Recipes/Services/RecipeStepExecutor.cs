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
                _eventBus.NotifyAsync<IRecipeExecuteEventHandler>(e =>
                    e.ExecutionCompleteAsync(executionId)).Wait();

                return await Task.FromResult(false);
            }

            _logger.LogInformation("Executing recipe step '{0}'.", nextRecipeStep.Name);

            var recipeContext = new RecipeContext
            {
                RecipeStep = nextRecipeStep,
                Executed = false,
                ExecutionId = executionId
            };

            try
            {
                _eventBus.NotifyAsync<IRecipeExecuteEventHandler>(e =>
                    e.RecipeStepExecutingAsync(executionId, recipeContext)).Wait();

                foreach (var recipeHandler in _recipeHandlers)
                {
                    recipeHandler.ExecuteRecipeStep(recipeContext);
                }

                UpdateStepResultRecord(executionId, nextRecipeStep.RecipeName, nextRecipeStep.Id, isSuccessful: true);
                _eventBus.NotifyAsync<IRecipeExecuteEventHandler>(e =>
                    e.RecipeStepExecutedAsync(executionId, recipeContext)).Wait();
            }
            catch (Exception ex)
            {
                UpdateStepResultRecord(executionId, nextRecipeStep.RecipeName, nextRecipeStep.Id, isSuccessful: false, errorMessage: ex.Message);
                _logger.LogError(string.Format("Recipe execution failed because the step '{0}' failed.", nextRecipeStep.Name), ex);
                while (await _recipeStepQueue.DequeueAsync(executionId) != null) ;
                var message = T["Recipe execution with ID {0} failed because the step '{1}' failed to execute. The following exception was thrown:\n{2}\nRefer to the error logs for more information.", executionId, nextRecipeStep.Name, ex.Message];
                throw new OrchardCoreException(message);
            }

            if (!recipeContext.Executed)
            {
                _logger.LogError("Recipe execution failed because no matching handler for recipe step '{0}' was found.", recipeContext.RecipeStep.Name);
                while (await _recipeStepQueue.DequeueAsync(executionId) != null) ;
                var message = T["Recipe execution with ID {0} failed because no matching handler for recipe step '{1}' was found. Refer to the error logs for more information.", executionId, nextRecipeStep.Name];
                throw new OrchardCoreException(message);
            }

            return true;
        }

        private void UpdateStepResultRecord(string executionId, string stepId, string stepName, bool isSuccessful, string errorMessage = null)
        {
            var stepResultRecord = _session
                .QueryAsync<RecipeStepResult>()
                .List()
                .Result
                .FirstOrDefault(
                    record => record.ExecutionId == executionId && record.StepId == stepId && record.StepName == stepName);

            if (stepResultRecord == null)
            {
                // No step result record was created when scheduling the step, so simply ignore.
                // The only reason where one would not create such a record would be Setup,
                // when no database exists to store the record but still wants to schedule a recipe step (such as the "StopViewsBackgroundCompilationStep").
                return;
            }

            stepResultRecord.IsCompleted = true;
            stepResultRecord.IsSuccessful = isSuccessful;
            stepResultRecord.ErrorMessage = errorMessage;

            _session.Save(stepResultRecord);
        }
    }
}
