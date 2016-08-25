using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;
using Orchard.Events;
using Orchard.FileSystem;
using Orchard.Hosting;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;
using YesSql.Core.Services;

namespace Orchard.Recipes.Services
{
    public class RecipeExecutor : IRecipeExecutor
    {
        private readonly IEventBus _eventBus;
        private readonly ISession _session;
        private readonly IRecipeParser _recipeParser;
        private readonly IOrchardFileSystem _fileSystem;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ILogger _logger;
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;

        public RecipeExecutor(
            IEventBus eventBus,
            ISession session,
            IRecipeParser recipeParser,
            IOrchardFileSystem fileSystem,
            IApplicationLifetime applicationLifetime,
            ShellSettings shellSettings,
            IOrchardHost orchardHost,
            ILogger<RecipeExecutor> logger,
            IStringLocalizer<RecipeExecutor> localizer)
        {
            _orchardHost = orchardHost;
            _shellSettings = shellSettings;
            _applicationLifetime = applicationLifetime;
            _eventBus = eventBus;
            _session = session;
            _recipeParser = recipeParser;
            _fileSystem = fileSystem;
            _logger = logger;
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public async Task<string> ExecuteAsync(string executionId, RecipeDescriptor recipeDescriptor)
        {
            await _eventBus.NotifyAsync<IRecipeEventHandler>(x => x.RecipeExecutingAsync(executionId, recipeDescriptor));

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
                    await _recipeParser.ProcessRecipeAsync(stream, async (recipe, recipeStep) =>
                    {
                        var shellContext = _orchardHost.GetOrCreateShellContext(_shellSettings);
                        using (var scope = shellContext.CreateServiceScope())
                        {
                            var recipeStepExecutor = scope.ServiceProvider.GetRequiredService<IRecipeStepExecutor>();

                            if (_applicationLifetime.ApplicationStopping.IsCancellationRequested)
                            {
                                throw new OrchardException(T["Recipe cancelled, application is restarting"]);
                            }

                            await recipeStepExecutor.ExecuteAsync(executionId, recipeStep);
                        }
                    });
                }

                await _eventBus.NotifyAsync<IRecipeEventHandler>(x => x.RecipeExecutedAsync(executionId, recipeDescriptor));

                return executionId;
            }
            catch (Exception)
            {
                await _eventBus.NotifyAsync<IRecipeEventHandler>(x => x.ExecutionFailedAsync(executionId, recipeDescriptor));

                throw;
            }
        }
    }
}