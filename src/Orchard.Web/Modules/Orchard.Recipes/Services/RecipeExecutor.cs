using Microsoft.Extensions.Logging;
using Orchard.DeferredTasks;
using Orchard.Environment.Shell.State;
using Orchard.Events;
using Orchard.FileSystem;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Recipes.Services
{
    public class RecipeExecutor : IRecipeExecutor
    {
        private readonly IEventBus _eventBus;
        private readonly ISession _session;
        private readonly IRecipeStepExecutor _recipeStepExecutor;
        private readonly IRecipeParser _recipeParser;
        private readonly ILogger _logger;

        private readonly IOrchardFileSystem _fileSystem;

        public RecipeExecutor(
            IEventBus eventBus,
            ISession session,
            IRecipeStepExecutor recipeStepExecutor,
            IRecipeParser recipeParser,
            IOrchardFileSystem fileSystem,
            ILogger<RecipeManager> logger)
        {
            _eventBus = eventBus;
            _session = session;
            _recipeStepExecutor = recipeStepExecutor;
            _recipeParser = recipeParser;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public async Task ExecuteAsync(string executionId, RecipeDescriptor recipeDescriptor)
        {
            _eventBus.Notify<IRecipeExecuteEventHandler>(x =>
                x.ExecutionStartAsync(executionId));

            await Task.Run(() =>
            {
                try
                {
                    using (var stream = _fileSystem.GetFileInfo(recipeDescriptor.Location).CreateReadStream())
                    {
                        RecipeResult result = new RecipeResult {
                            ExecutionId = executionId
                        };
                        List<RecipeStepResult> stepResults = new List<RecipeStepResult>();
                        _recipeParser.ProcessRecipe(
                            stream
                            , (recipe, recipeStep) =>
                            {
                                // TODO, create Result prior to run
                                stepResults.Add(new RecipeStepResult
                                {
                                    ExecutionId = executionId,
                                    RecipeName = recipeDescriptor.Name,
                                    StepId = recipeStep.Id,
                                    StepName = recipeStep.Name
                                });
                            });
                        result.Steps = stepResults;
                        _session.Save(result);
                    }

                    using (var stream = _fileSystem.GetFileInfo(recipeDescriptor.Location).CreateReadStream())
                    {
                        _recipeParser.ProcessRecipe(
                            stream
                            , async (recipe, recipeStep) =>
                            {
                                await _recipeStepExecutor.ExecuteAsync(executionId, recipeStep);
                            });
                    }

                    _eventBus.Notify<IRecipeExecuteEventHandler>(x =>
                        x.ExecutionCompleteAsync(executionId));

                    return executionId;
                }
                catch (Exception)
                {
                    _eventBus.Notify<IRecipeExecuteEventHandler>(x =>
                        x.ExecutionFailedAsync(executionId));

                    throw;
                }
            });
        }
    }
}