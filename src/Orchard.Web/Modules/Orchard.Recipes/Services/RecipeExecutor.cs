using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.DeferredTasks;
using Orchard.Environment.Shell;
using Orchard.Events;
using Orchard.FileSystem;
using Orchard.Hosting;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;
using YesSql.Core.Services;
using Microsoft.Extensions.Options;
using System.IO;

namespace Orchard.Recipes.Services
{
    public class RecipeExecutor : IRecipeExecutor
    {
        private readonly IEventBus _eventBus;
        private readonly ISession _session;
        private readonly IEnumerable<IRecipeParser> _recipeParsers;
        private readonly RecipeHarvestingOptions _recipeOptions;
        private readonly IOrchardFileSystem _fileSystem;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ILogger _logger;
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;

        public RecipeExecutor(
            IEventBus eventBus,
            ISession session,
            IEnumerable<IRecipeParser> recipeParsers,
            IOptions<RecipeHarvestingOptions> recipeOptions,
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
            _recipeParsers = recipeParsers;
            _recipeOptions = recipeOptions.Value;
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
                var parsersForFileExtension = _recipeOptions
                    .RecipeFileExtensions
                    .Where(rfx => Path.GetExtension(rfx.Key) == Path.GetExtension(recipeDescriptor.RecipeFileInfo.PhysicalPath));

                using (var stream = recipeDescriptor.RecipeFileInfo.CreateReadStream())
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
                    _session.Save(result);
                }

                using (var stream = recipeDescriptor.RecipeFileInfo.CreateReadStream())
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