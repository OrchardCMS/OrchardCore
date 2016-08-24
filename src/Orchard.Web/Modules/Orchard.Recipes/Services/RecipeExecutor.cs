using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Events;
using Orchard.FileSystem;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;
using YesSql.Core.Services;

namespace Orchard.Recipes.Services
{
    public class RecipeExecutor : IRecipeExecutor
    {
        private readonly IEventBus _eventBus;
        private readonly ISession _session;
        private readonly IRecipeStepExecutor _recipeStepExecutor;
        private readonly IRecipeParser _recipeParser;
        private readonly IOrchardFileSystem _fileSystem;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ILogger _logger;

        public RecipeExecutor(
            IEventBus eventBus,
            ISession session,
            IRecipeStepExecutor recipeStepExecutor,
            IRecipeParser recipeParser,
            IOrchardFileSystem fileSystem,
            IApplicationLifetime applicationLifetime,
            ILogger<RecipeExecutor> logger,
            IStringLocalizer<RecipeExecutor> localizer)
        {
            _applicationLifetime = applicationLifetime;
            _eventBus = eventBus;
            _session = session;
            _recipeStepExecutor = recipeStepExecutor;
            _recipeParser = recipeParser;
            _fileSystem = fileSystem;
            _logger = logger;
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public async Task<string> ExecuteAsync(string executionId, RecipeDescriptor recipeDescriptor)
        {
            _eventBus.Notify<IRecipeEventHandler>(x => x.RecipeExecutingAsync(executionId, recipeDescriptor));

            try
            {
                using (var stream = _fileSystem.GetFileInfo(recipeDescriptor.Location).CreateReadStream())
                {
                    RecipeResult result = new RecipeResult { ExecutionId = executionId };
                    List<RecipeStepResult> stepResults = new List<RecipeStepResult>();

                    await _recipeParser.ProcessRecipeAsync(stream, (recipe, recipeStep) =>
                    {
                        // TODO, create Result prior to run
                        stepResults.Add(new RecipeStepResult
                        {
                            ExecutionId = executionId,
                            RecipeName = recipeDescriptor.Name,
                            StepId = recipeStep.Id,
                            StepName = recipeStep.Name
                        });
                        return Task.CompletedTask;
                    });

                    result.Steps = stepResults;
                    _session.Save(result);
                }

                using (var stream = _fileSystem.GetFileInfo(recipeDescriptor.Location).CreateReadStream())
                {
                    await _recipeParser.ProcessRecipeAsync(stream, (recipe, recipeStep) =>
                    {
                        if (_applicationLifetime.ApplicationStopping.IsCancellationRequested)
                        {
                            throw new OrchardException(T["Recipe cancelled, application is restarting"]);
                        }

                        return _recipeStepExecutor.ExecuteAsync(executionId, recipeStep);
                    });
                }

                _eventBus.Notify<IRecipeEventHandler>(x => x.RecipeExecutedAsync(executionId, recipeDescriptor));

                return executionId;
            }
            catch (Exception)
            {
                _eventBus.Notify<IRecipeEventHandler>(x => x.ExecutionFailedAsync(executionId, recipeDescriptor));

                throw;
            }
        }
    }
}