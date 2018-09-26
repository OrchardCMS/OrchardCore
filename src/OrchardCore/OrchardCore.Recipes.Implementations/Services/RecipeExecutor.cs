using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.DeferredTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Recipes.Events;
using OrchardCore.Recipes.Models;
using OrchardCore.Scripting;

namespace OrchardCore.Recipes.Services
{
    public class RecipeExecutor : IRecipeExecutor
    {
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ShellSettings _shellSettings;
        private readonly IShellHost _orchardHost;
        private readonly IEnumerable<IRecipeEventHandler> _recipeEventHandlers;
        private readonly IRecipeStore _recipeStore;

        private VariablesMethodProvider _variablesMethodProvider;
        private ParametersMethodProvider _environmentMethodProvider;

        public RecipeExecutor(
            IEnumerable<IRecipeEventHandler> recipeEventHandlers,
            IRecipeStore recipeStore,
            IApplicationLifetime applicationLifetime,
            ShellSettings shellSettings,
            IShellHost orchardHost,
            ILogger<RecipeExecutor> logger,
            IStringLocalizer<RecipeExecutor> localizer)
        {
            _orchardHost = orchardHost;
            _shellSettings = shellSettings;
            _applicationLifetime = applicationLifetime;
            _recipeEventHandlers = recipeEventHandlers;
            _recipeStore = recipeStore;
            Logger = logger;
            T = localizer;
        }

        public ILogger Logger { get; set; }
        public IStringLocalizer T { get; set; }

        public async Task<string> ExecuteAsync(string executionId, RecipeDescriptor recipeDescriptor, object environment)
        {
            await _recipeEventHandlers.InvokeAsync(x => x.RecipeExecutingAsync(executionId, recipeDescriptor), Logger);

            try
            {
                _environmentMethodProvider = new ParametersMethodProvider(environment);

                var result = new RecipeResult { ExecutionId = executionId };

                await _recipeStore.CreateAsync(result);

                using (var stream = recipeDescriptor.RecipeFileInfo.CreateReadStream())
                {
                    using (var file = new StreamReader(stream))
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
                                                Environment = environment,
                                                RecipeDescriptor = recipeDescriptor
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
                                            catch (Exception e)
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

                                            if (recipeStep.InnerRecipes != null)
                                            {
                                                foreach (var descriptor in recipeStep.InnerRecipes)
                                                {
                                                    var innerExecutionId = Guid.NewGuid().ToString();
                                                    await ExecuteAsync(innerExecutionId, descriptor, environment);
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                await _recipeEventHandlers.InvokeAsync(x => x.RecipeExecutedAsync(executionId, recipeDescriptor), Logger);

                return executionId;
            }
            catch (Exception)
            {
                await _recipeEventHandlers.InvokeAsync(x => x.ExecutionFailedAsync(executionId, recipeDescriptor), Logger);

                throw;
            }
        }

        private async Task ExecuteStepAsync(RecipeExecutionContext recipeStep)
        {
            var (scope, shellContext) = await _orchardHost.GetScopeAndContextAsync(_shellSettings);

            using (scope)
            {
                if (!shellContext.IsActivated)
                {
                    using (var activatingScope = shellContext.CreateScope())
                    {
                        var tenantEvents = activatingScope.ServiceProvider.GetServices<IModularTenantEvents>();

                        foreach (var tenantEvent in tenantEvents)
                        {
                            await tenantEvent.ActivatingAsync();
                        }

                        foreach (var tenantEvent in tenantEvents.Reverse())
                        {
                            await tenantEvent.ActivatedAsync();
                        }
                    }

                    shellContext.IsActivated = true;
                }

                var recipeStepHandlers = scope.ServiceProvider.GetServices<IRecipeStepHandler>();
                var scriptingManager = scope.ServiceProvider.GetRequiredService<IScriptingManager>();
                scriptingManager.GlobalMethodProviders.Add(_environmentMethodProvider);

                // Substitutes the script elements by their actual values
                EvaluateScriptNodes(recipeStep, scriptingManager);

                foreach (var recipeStepHandler in recipeStepHandlers)
                {
                    if (Logger.IsEnabled(LogLevel.Information))
                    {
                        Logger.LogInformation("Executing recipe step '{RecipeName}'.", recipeStep.Name);
                    }

                    await _recipeEventHandlers.InvokeAsync(e => e.RecipeStepExecutingAsync(recipeStep), Logger);

                    await recipeStepHandler.ExecuteAsync(recipeStep);

                    await _recipeEventHandlers.InvokeAsync(e => e.RecipeStepExecutedAsync(recipeStep), Logger);

                    if (Logger.IsEnabled(LogLevel.Information))
                    {
                        Logger.LogInformation("Finished executing recipe step '{RecipeName}'.", recipeStep.Name);
                    }
                }
            }

            // The recipe execution might have invalidated the shell by enabling new features,
            // so the deferred tasks need to run on an updated shell context if necessary.
            using (var localScope = await _orchardHost.GetScopeAsync(_shellSettings))
            {
                var deferredTaskEngine = localScope.ServiceProvider.GetService<IDeferredTaskEngine>();

                // The recipe might have added some deferred tasks to process
                if (deferredTaskEngine != null && deferredTaskEngine.HasPendingTasks)
                {
                    var taskContext = new DeferredTaskContext(localScope.ServiceProvider);
                    await deferredTaskEngine.ExecuteTasksAsync(taskContext);
                }
            }
        }

        /// <summary>
        /// Traverse all the nodes of the recipe steps and replaces their value if they are scripted.
        /// </summary>
        private void EvaluateScriptNodes(RecipeExecutionContext context, IScriptingManager scriptingManager)
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

            EvaluateJsonTree(scriptingManager, context, context.Step);
        }

        /// <summary>
        /// Traverse all the nodes of the json document and replaces their value if they are scripted.
        /// </summary>
        private void EvaluateJsonTree(IScriptingManager scriptingManager, RecipeExecutionContext context, JToken node)
        {
            switch (node.Type)
            {
                case JTokenType.Array:
                    var array = (JArray)node;
                    for (var i = 0; i < array.Count; i++)
                    {
                        EvaluateJsonTree(scriptingManager, context, array[i]);
                    }
                    break;
                case JTokenType.Object:
                    foreach (var property in (JObject)node)
                    {
                        EvaluateJsonTree(scriptingManager, context, property.Value);
                    }
                    break;

                case JTokenType.String:

                    var value = node.Value<string>();

                    // Evaluate the expression while the result is another expression
                    while (value.StartsWith("[") && value.EndsWith("]"))
                    {
                        value = value.Trim('[', ']');
                        value = (scriptingManager.Evaluate(value, context.RecipeDescriptor.FileProvider, context.RecipeDescriptor.BasePath, null) ?? "").ToString();
                        ((JValue)node).Value = value;
                    }
                    break;
            }
        }
    }
}
