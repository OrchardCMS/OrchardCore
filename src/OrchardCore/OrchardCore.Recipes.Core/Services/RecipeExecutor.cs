using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using OrchardCore.Recipes.Events;
using OrchardCore.Recipes.Models;
using OrchardCore.Scripting;

namespace OrchardCore.Recipes.Services
{
    public class RecipeExecutor : IRecipeExecutor
    {
        private readonly ShellSettings _shellSettings;
        private readonly IShellHost _shellHost;
        private readonly IEnumerable<IRecipeEventHandler> _recipeEventHandlers;
        private readonly ILogger _logger;

        private VariablesMethodProvider _variablesMethodProvider;
        private ConfigurationMethodProvider _configurationMethodProvider;
        private ParametersMethodProvider _environmentMethodProvider;

        public RecipeExecutor(IEnumerable<IRecipeEventHandler> recipeEventHandlers,
                              ShellSettings shellSettings,
                              IShellHost shellHost,
                              ILogger<RecipeExecutor> logger)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _recipeEventHandlers = recipeEventHandlers;
            _logger = logger;
        }

        public async Task<string> ExecuteAsync(string executionId, RecipeDescriptor recipeDescriptor, object environment, CancellationToken cancellationToken)
        {
            await _recipeEventHandlers.InvokeAsync((handler, executionId, recipeDescriptor) => handler.RecipeExecutingAsync(executionId, recipeDescriptor), executionId, recipeDescriptor, _logger);

            try
            {
                _environmentMethodProvider = new ParametersMethodProvider(environment);
                _configurationMethodProvider = new ConfigurationMethodProvider(_shellSettings.ShellConfiguration);

                var result = new RecipeResult { ExecutionId = executionId };

                using (var stream = recipeDescriptor.RecipeFileInfo.CreateReadStream())
                {
                    using (var file = new StreamReader(stream))
                    {
                        using (var reader = new JsonTextReader(file))
                        {
                            // Go to Steps, then iterate.
                            while (await reader.ReadAsync())
                            {
                                if (reader.Path == "variables")
                                {
                                    await reader.ReadAsync();

                                    var variables = await JObject.LoadAsync(reader);
                                    _variablesMethodProvider = new VariablesMethodProvider(variables);
                                }

                                if (reader.Path == "steps" && reader.TokenType == JsonToken.StartArray)
                                {
                                    while (await reader.ReadAsync() && reader.Depth > 1)
                                    {
                                        if (reader.Depth == 2)
                                        {
                                            var child = await JObject.LoadAsync(reader);

                                            var recipeStep = new RecipeExecutionContext
                                            {
                                                Name = child.Value<string>("name"),
                                                Step = child,
                                                ExecutionId = executionId,
                                                Environment = environment,
                                                RecipeDescriptor = recipeDescriptor
                                            };

                                            if (cancellationToken.IsCancellationRequested)
                                            {
                                                _logger.LogError("Recipe interrupted by cancellation token.");
                                                return null;
                                            }

                                            var stepResult = new RecipeStepResult { StepName = recipeStep.Name };
                                            result.Steps.Add(stepResult);

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

                                            if (stepResult.IsSuccessful == false)
                                            {
                                                capturedException.Throw();
                                            }

                                            if (recipeStep.InnerRecipes != null)
                                            {
                                                foreach (var descriptor in recipeStep.InnerRecipes)
                                                {
                                                    var innerExecutionId = Guid.NewGuid().ToString();
                                                    await ExecuteAsync(innerExecutionId, descriptor, environment, cancellationToken);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                await _recipeEventHandlers.InvokeAsync((handler, executionId, recipeDescriptor) => handler.RecipeExecutedAsync(executionId, recipeDescriptor), executionId, recipeDescriptor, _logger);

                return executionId;
            }
            catch (Exception)
            {
                await _recipeEventHandlers.InvokeAsync((handler, executionId, recipeDescriptor) => handler.ExecutionFailedAsync(executionId, recipeDescriptor), executionId, recipeDescriptor, _logger);

                throw;
            }
        }

        private async Task ExecuteStepAsync(RecipeExecutionContext recipeStep)
        {
            var shellScope = recipeStep.RecipeDescriptor.RequireNewScope
                ? await _shellHost.GetScopeAsync(_shellSettings)
                : ShellScope.Current;

            await shellScope.UsingAsync(async scope =>
            {
                var recipeStepHandlers = scope.ServiceProvider.GetServices<IRecipeStepHandler>();
                var scriptingManager = scope.ServiceProvider.GetRequiredService<IScriptingManager>();
                scriptingManager.GlobalMethodProviders.Add(_environmentMethodProvider);
                scriptingManager.GlobalMethodProviders.Add(_configurationMethodProvider);

                // Substitutes the script elements by their actual values
                EvaluateScriptNodes(recipeStep, scriptingManager);

                foreach (var recipeStepHandler in recipeStepHandlers)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Executing recipe step '{RecipeName}'.", recipeStep.Name);
                    }

                    await _recipeEventHandlers.InvokeAsync((handler, recipeStep) => handler.RecipeStepExecutingAsync(recipeStep), recipeStep, _logger);

                    await recipeStepHandler.ExecuteAsync(recipeStep);

                    await _recipeEventHandlers.InvokeAsync((handler, recipeStep) => handler.RecipeStepExecutedAsync(recipeStep), recipeStep, _logger);

                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Finished executing recipe step '{RecipeName}'.", recipeStep.Name);
                    }
                }
            });
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
                    while (value.StartsWith('[') && value.EndsWith(']'))
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
