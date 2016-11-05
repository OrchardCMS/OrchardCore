using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Events;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public class RecipeStepExecutor : IRecipeStepExecutor
    {
        private readonly IEnumerable<IRecipeHandler> _recipeHandlers;
        private readonly IEventBus _eventBus;
        private readonly ILogger _logger;
        private readonly IRecipeStore _recipeStore;

        public RecipeStepExecutor(
            IEnumerable<IRecipeHandler> recipeHandlers,
            IEventBus eventBus,
            IRecipeStore recipeStore,
            ILogger<RecipeStepExecutor> logger,
            IStringLocalizer<RecipeStepExecutor> localizer)
        {
            _recipeHandlers = recipeHandlers;
            _eventBus = eventBus;
            _recipeStore = recipeStore;
            _logger = logger;

            T = localizer;
        }

        public IStringLocalizer T { get; }

        public async Task ExecuteAsync(string executionId, RecipeStepDescriptor recipeStep)
        {
            _logger.LogInformation("Executing recipe step '{0}'.", recipeStep.Name);

            var recipeContext = new RecipeContext
            {
                RecipeStep = recipeStep,
                Executed = false,
                ExecutionId = executionId
            };

            await _eventBus
                .NotifyAsync<IRecipeEventHandler>(e => e.RecipeStepExecutingAsync(executionId, recipeStep));

            IList<Task> tasks = new List<Task>();
            foreach (var handler in _recipeHandlers)
            {
                tasks.Add(handler.ExecuteRecipeStepAsync(recipeContext));
            }
            Task.WaitAll(tasks.ToArray());

            UpdateStepResultRecordAsync(recipeContext, true).Wait();

            await _eventBus
                .NotifyAsync<IRecipeEventHandler>(e => e.RecipeStepExecutedAsync(executionId, recipeStep));
        }

        private async Task UpdateStepResultRecordAsync(
            RecipeContext recipeContext,
            bool IsSuccessful,
            Exception exception = null)
        {
            var recipeResult = await _recipeStore.FindByExecutionIdAsync(recipeContext.ExecutionId);

            if (recipeResult != null)
            {
                var recipeStepResult = recipeResult.Steps.FirstOrDefault(step => step.StepId == recipeContext.RecipeStep.Id);

                if (recipeStepResult != null)
                {
                    recipeStepResult.IsCompleted = true;
                    recipeStepResult.IsSuccessful = IsSuccessful;
                    recipeStepResult.ErrorMessage = exception?.ToString();

                    await _recipeStore.UpdateAsync(recipeResult);
                }
            }
        }
    }
}