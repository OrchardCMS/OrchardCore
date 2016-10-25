using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.DeferredTasks;
using Orchard.Environment.Shell;
using Orchard.Events;
using Orchard.Hosting;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public class RecipeExecutor : IRecipeExecutor
    {
        private readonly IEventBus _eventBus;
        private readonly IEnumerable<IRecipeParser> _recipeParsers;
        private readonly RecipeHarvestingOptions _recipeOptions;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ILogger _logger;
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;
        private readonly IRecipeStore _recipeStore;

        public RecipeExecutor(
            IEventBus eventBus,
            IRecipeStore recipeStore,
            IEnumerable<IRecipeParser> recipeParsers,
            IOptions<RecipeHarvestingOptions> recipeOptions,
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
            _recipeStore = recipeStore;
            _recipeParsers = recipeParsers;
            _recipeOptions = recipeOptions.Value;
            _logger = logger;
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public async Task<string> ExecuteAsync(string executionId, RecipeDescriptor recipeDescriptor)
        {
            await _eventBus.NotifyAsync<IRecipeEventHandler>(x => x.RecipeExecutingAsync(executionId, recipeDescriptor));

            try
            {
                var parsersForFileExtension = _recipeOptions
                    .RecipeFileExtensions
                    .Where(rfx => Path.GetExtension(rfx.Key) == Path.GetExtension(recipeDescriptor.RecipeFileProvider.GetFileInfo(recipeDescriptor.RecipeFileName).PhysicalPath));

                using (var stream = recipeDescriptor.RecipeFileProvider.GetFileInfo(recipeDescriptor.RecipeFileName).CreateReadStream())
                {
                    RecipeResult result = new RecipeResult { ExecutionId = executionId };
                    List<RecipeStepResult> stepResults = new List<RecipeStepResult>();

                    foreach (var parserForFileExtension in parsersForFileExtension)
                    {
                        var recipeParser = _recipeParsers.First(x => x.GetType() == parserForFileExtension.Value);

                        await recipeParser.ProcessRecipeAsync(stream, (recipe, recipeStep) =>
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
                    }

                    result.Steps = stepResults;
                    await _recipeStore.UpdateAsync(result);
                }

                using (var stream = recipeDescriptor.RecipeFileProvider.GetFileInfo("Recipe.json").CreateReadStream())
                {
                    foreach (var parserForFileExtension in parsersForFileExtension)
                    {
                        var recipeParser = _recipeParsers.First(x => x.GetType() == parserForFileExtension.Value);

                        await recipeParser.ProcessRecipeAsync(stream, async (recipe, recipeStep) =>
                        {
                            var shellContext = _orchardHost.GetOrCreateShellContext(_shellSettings);
                            using (var scope = shellContext.CreateServiceScope())
                            {
                                if (!shellContext.IsActivated)
                                {
                                    var eventBus = scope.ServiceProvider.GetService<IEventBus>();
                                    await eventBus.NotifyAsync<IOrchardShellEvents>(x => x.ActivatingAsync());
                                    await eventBus.NotifyAsync<IOrchardShellEvents>(x => x.ActivatedAsync());

                                    shellContext.IsActivated = true;
                                }

                                var recipeStepExecutor = scope.ServiceProvider.GetRequiredService<IRecipeStepExecutor>();

                                if (_applicationLifetime.ApplicationStopping.IsCancellationRequested)
                                {
                                    throw new OrchardException(T["Recipe cancelled, application is restarting"]);
                                }

                                await recipeStepExecutor.ExecuteAsync(executionId, recipeStep);
                            }

                            // The recipe execution might have invalidated the shell by enabling new features,
                            // so the deferred tasks need to run on an updated shell context if necessary.
                            shellContext = _orchardHost.GetOrCreateShellContext(_shellSettings);
                            using (var scope = shellContext.CreateServiceScope())
                            {
                                var deferredTaskEngine = scope.ServiceProvider.GetService<IDeferredTaskEngine>();

                                // The recipe might have added some deferred tasks to process
                                if (deferredTaskEngine != null && deferredTaskEngine.HasPendingTasks)
                                {
                                    var taskContext = new DeferredTaskContext(scope.ServiceProvider);
                                    await deferredTaskEngine.ExecuteTasksAsync(taskContext);
                                }
                            }
                        });
                    }
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