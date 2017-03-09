using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.DeferredTasks;
using Orchard.Environment.Shell;
using Orchard.Events;
using Orchard.Hosting;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;
using Orchard.Scripting;

namespace Orchard.Recipes.Services
{
    public class RecipeExecutor : IRecipeExecutor
    {
        private readonly IEventBus _eventBus;
        private readonly RecipeHarvestingOptions _recipeOptions;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ShellSettings _shellSettings;
        private readonly IShellHost _orchardHost;
        private readonly IRecipeStore _recipeStore;

        private VariablesMethodProvider _variablesMethodProvider;
        private ParametersMethodProvider _environmentMethodProvider;

        public RecipeExecutor(
            IEventBus eventBus,
            IRecipeStore recipeStore,
            IOptions<RecipeHarvestingOptions> recipeOptions,
            IApplicationLifetime applicationLifetime,
            ShellSettings shellSettings,
            IShellHost orchardHost,
            ILogger<RecipeExecutor> logger,
            IStringLocalizer<RecipeExecutor> localizer)
        {
            _orchardHost = orchardHost;
            _shellSettings = shellSettings;
            _applicationLifetime = applicationLifetime;
            _eventBus = eventBus;
            _recipeStore = recipeStore;
            _recipeOptions = recipeOptions.Value;
            Logger = logger;
            T = localizer;
        }

        public ILogger Logger { get; set; }
        public IStringLocalizer T { get; set; }

        public async Task<string> ExecuteAsync(string executionId, RecipeDescriptor recipeDescriptor, object environment)
        {
            await _eventBus.NotifyAsync<IRecipeEventHandler>(x => x.RecipeExecutingAsync(executionId, recipeDescriptor));

            try
            {
                _environmentMethodProvider = new ParametersMethodProvider(environment);

                var result = new RecipeResult { ExecutionId = executionId };

                await _recipeStore.CreateAsync(result);

                using (StreamReader file = File.OpenText(recipeDescriptor.RecipeFileInfo.PhysicalPath))
                {
                    using (var reader = new JsonTextReader(file))
                    {
                        // Go to Steps, then iterate.
                        while (reader.Read())
                        {
                            if (reader.Path == "variables")
                            {
                                reader.Read();

                                var variables = JObject.Load(reader);
                                _variablesMethodProvider = new VariablesMethodProvider(variables);
                            }

                            if (reader.Path == "steps" && reader.TokenType == JsonToken.StartArray)
                            {
                                while (reader.Read() && reader.Depth > 1)
                                {
                                    if (reader.Depth == 2)
                                    {
                                        var child = JObject.Load(reader);

                                        var recipeStep = new RecipeExecutionContext
                                        {
                                            Name = child.Value<string>("name"),
                                            Step = child,
                                            ExecutionId = executionId,
                                            Environment = environment
                                        };

                                        var stepResult = new RecipeStepResult { StepName = recipeStep.Name };
                                        result.Steps.Add(stepResult);
                                        await _recipeStore.UpdateAsync(result);

                                        ExceptionDispatchInfo capturedException = null;
                                        try
                                        {
                                            await ExecuteStepAsync(recipeStep);
                                            stepResult.IsSuccessful = true;
                                        }
                                        catch(Exception e)
                                        {
                                            stepResult.IsSuccessful = false;
                                            stepResult.ErrorMessage = e.ToString();

                                            // Because we can't do some async processing the in catch or finally
                                            // blocks, we store the exception to throw it later.

                                            capturedException = ExceptionDispatchInfo.Capture(e);
                                        }

                                        stepResult.IsCompleted = true;
                                        await _recipeStore.UpdateAsync(result);

                                        if (stepResult.IsSuccessful == false)
                                        {
                                            capturedException.Throw();
                                        }
                                    }
                                }
                            }
                        }
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

        private async Task ExecuteStepAsync(RecipeExecutionContext recipeStep)
        {
            var shellContext = _orchardHost.GetOrCreateShellContext(_shellSettings);
            using (var scope = shellContext.CreateServiceScope())
            {
                if (!shellContext.IsActivated)
                {
                    var tenantEvents = scope.ServiceProvider
                        .GetServices<IModularTenantEvents>();

                    foreach (var tenantEvent in tenantEvents)
                    {
                        tenantEvent.ActivatingAsync().Wait();
                    }

                    shellContext.IsActivated = true;

                    foreach (var tenantEvent in tenantEvents)
                    {
                        tenantEvent.ActivatedAsync().Wait();
                    }
                }

                var recipeStepHandlers = scope.ServiceProvider.GetRequiredService<IEnumerable<IRecipeStepHandler>>();
                var scriptingManager = scope.ServiceProvider.GetRequiredService<IScriptingManager>();
                scriptingManager.GlobalMethodProviders.Add(_environmentMethodProvider);

                // Substitutes the script elements by their actual values
                EvaluateScriptNodes(recipeStep, scriptingManager);

                foreach (var recipeStepHandler in recipeStepHandlers)
                {
                    if (Logger.IsEnabled(LogLevel.Information))
                    {
                        Logger.LogInformation("Executing recipe step '{0}'.", recipeStep.Name);
                    }

                    await _eventBus.NotifyAsync<IRecipeEventHandler>(e => e.RecipeStepExecutingAsync(recipeStep));

                    await recipeStepHandler.ExecuteAsync(recipeStep);

                    await _eventBus.NotifyAsync<IRecipeEventHandler>(e => e.RecipeStepExecutedAsync(recipeStep));

                    if (Logger.IsEnabled(LogLevel.Information))
                    {
                        Logger.LogInformation("Finished executing recipe step '{0}'.", recipeStep.Name);
                    }
                }
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
        }

        /// <summary>
        /// Traverse all the nodes of the recipe steps and replaces their value if they are scripted.
        /// </summary>
        private void EvaluateScriptNodes(RecipeExecutionContext recipeStep, IScriptingManager scriptingManager)
        {
            if (_variablesMethodProvider != null)
            {
                _variablesMethodProvider.ScriptingManager = scriptingManager;
                scriptingManager.GlobalMethodProviders.Add(_variablesMethodProvider);
            }

            if (_applicationLifetime.ApplicationStopping.IsCancellationRequested)
            {
                throw new Exception(T["Recipe cancelled, application is restarting"]);
            }

            EvaluateJsonTree(scriptingManager, recipeStep.Step);
        }

        /// <summary>
        /// Traverse all the nodes of the json document and replaces their value if they are scripted.
        /// </summary>
        private void EvaluateJsonTree(IScriptingManager scriptingManager, JToken node)
        {
            switch (node.Type)
            {
                case JTokenType.Array:
                    var array = (JArray)node;
                    for (var i=0; i < array.Count; i++)
                    {
                        EvaluateJsonTree(scriptingManager, array[i]);
                    }
                    break;
                case JTokenType.Object:
                    foreach (var property in (JObject)node)
                    {
                        EvaluateJsonTree(scriptingManager, property.Value);
                    }
                    break;

                case JTokenType.String:

                    var value = node.Value<string>();

                    // Evaluate the expression while the result is another expression
                    while (value.StartsWith("[") && value.EndsWith("]"))
                    {
                        value = value.Trim('[', ']');
                        value = (scriptingManager.Evaluate(value) ?? "").ToString();
                        ((JValue)node).Value = value;
                    }
                    break;
            }
        }
    }
}