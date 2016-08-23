using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Recipes.Providers.RecipeHandlers
{
    /// <summary>
    /// Delegates execution of the step to the appropriate recipe execution step implementation.
    /// </summary>
    public class RecipeExecutionStepHandler : IRecipeHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        public RecipeExecutionStepHandler(
            IServiceProvider serviceProvider,
            ILogger<RecipeExecutionStepHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void ExecuteRecipeStep(RecipeContext recipeContext)
        {
            var recipeExecutionSteps = _serviceProvider.GetServices<IRecipeExecutionStep>();

            var executionStep = recipeExecutionSteps
                .FirstOrDefault(x => x.Names.Contains(recipeContext.RecipeStep.Name, StringComparer.OrdinalIgnoreCase));

            if (executionStep != null)
            {
                var recipeExecutionContext = new RecipeExecutionContext
                {
                    ExecutionId = recipeContext.ExecutionId,
                    RecipeStep = recipeContext.RecipeStep
                };

                _logger.LogInformation("Executing recipe step '{0}'.", recipeContext.RecipeStep.Name);
                executionStep.Execute(recipeExecutionContext);
                _logger.LogInformation("Finished executing recipe step '{0}'.", recipeContext.RecipeStep.Name);
                recipeContext.Executed = true;
            }
        }
    }
}
