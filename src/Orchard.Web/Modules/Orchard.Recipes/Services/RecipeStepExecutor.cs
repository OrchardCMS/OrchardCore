using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Events;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;
using YesSql.Core.Services;

namespace Orchard.Recipes.Services
{
    public class RecipeStepExecutor : IRecipeStepExecutor
    {
        private readonly IEnumerable<IRecipeHandler> _recipeHandlers;
        private readonly IEventBus _eventBus;
        private readonly ISession _session;
        private readonly ILogger _logger;

        public RecipeStepExecutor(
            IEnumerable<IRecipeHandler> recipeHandlers,
            IEventBus eventBus,
            ISession session,
            ILogger<RecipeStepExecutor> logger,
            IStringLocalizer<RecipeStepExecutor> localizer)
        {
            _recipeHandlers = recipeHandlers;
            _eventBus = eventBus;
            _session = session;
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

            await _eventBus.NotifyAsync<IRecipeEventHandler>(e => e.RecipeStepExecutingAsync(executionId, recipeStep));

            await _recipeHandlers.InvokeAsync(rh => rh.ExecuteRecipeStepAsync(recipeContext), _logger);

            await UpdateStepResultRecordAsync(recipeContext, true);

            await _eventBus.NotifyAsync<IRecipeEventHandler>(e => e.RecipeStepExecutedAsync(executionId, recipeStep));
        }

        private Task UpdateStepResultRecordAsync(
            RecipeContext recipeContext,
            bool IsSuccessful,
            Exception exception = null)
        {
            // TODO: bad pattern, use an index

            //var stepResults = await _session
            //    .QueryAsync<RecipeResult>()
            //    .List();

            //var recipeResult = stepResults
            //    .First(record => record.ExecutionId == recipeContext.ExecutionId);

            //var recipeStepResult = recipeResult
            //    .Steps
            //    .First(step => step.StepId == recipeContext.RecipeStep.Id);

            //recipeStepResult.IsCompleted = true;
            //recipeStepResult.IsSuccessful = IsSuccessful;
            //recipeStepResult.ErrorMessage = exception?.ToString();

            //_session.Save(recipeResult);

            return Task.CompletedTask;
        }
    }
}