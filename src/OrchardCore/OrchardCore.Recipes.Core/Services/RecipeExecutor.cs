using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using OrchardCore.Recipes.Events;
using OrchardCore.Recipes.Models;
using OrchardCore.Scripting;

namespace OrchardCore.Recipes.Services;

public class RecipeExecutor : IRecipeExecutor
{
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly IEnumerable<IRecipeEventHandler> _recipeEventHandlers;
    private readonly ILogger _logger;
    private readonly Dictionary<string, List<IGlobalMethodProvider>> _methodProviders = [];

    protected readonly IStringLocalizer S;

    public RecipeExecutor(
        IShellHost shellHost,
        ShellSettings shellSettings,
        IEnumerable<IRecipeEventHandler> recipeEventHandlers,
        ILogger<RecipeExecutor> logger,
        IStringLocalizer<RecipeExecutor> stringLocalizer)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _recipeEventHandlers = recipeEventHandlers;
        _logger = logger;
        S = stringLocalizer;
    }

    public async Task<string> ExecuteAsync(string executionId, IRecipeDescriptor recipeDescriptor, IDictionary<string, object> environment, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(recipeDescriptor);

        var legacyDescriptor = recipeDescriptor as RecipeDescriptor;

        if (legacyDescriptor is not null)
        {
            await _recipeEventHandlers.InvokeAsync((handler, executionId, legacyDescriptor) => handler.RecipeExecutingAsync(executionId, legacyDescriptor), executionId, legacyDescriptor, _logger);
        }

        try
        {
            var methodProviders = new List<IGlobalMethodProvider>();
            _methodProviders.Add(executionId, methodProviders);

            methodProviders.Add(new ParametersMethodProvider(environment));
            methodProviders.Add(new ConfigurationMethodProvider(_shellSettings.ShellConfiguration));

            var result = new RecipeResult { ExecutionId = executionId };

            await using (var stream = await recipeDescriptor.OpenReadStreamAsync())
            {
                using var doc = await JsonDocument.ParseAsync(stream, JOptions.Document, cancellationToken);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException($"Top-level JSON element must be an object. Instead, '{doc.RootElement.ValueKind}' was found.");
                }

                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    if (property.Name == "variables")
                    {
                        var variables = JsonObject.Create(property.Value);
                        methodProviders.Add(new VariablesMethodProvider(variables, methodProviders));
                    }

                    // Go to Steps, then iterate.
                    if (property.Name == "steps" && property.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var step in property.Value.EnumerateArray())
                        {
                            var child = JsonObject.Create(step);

                            var recipeStep = new RecipeExecutionContext
                            {
                                Name = child.Value<string>("name"),
                                Step = child,
                                ExecutionId = executionId,
                                Environment = environment,
                                RecipeDescriptor = recipeDescriptor,
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

                                if (recipeStep.Errors.Count > 0)
                                {
                                    stepResult.IsSuccessful = false;
                                    stepResult.Errors = recipeStep.Errors.ToArray();
                                }
                                else
                                {
                                    stepResult.IsSuccessful = true;
                                }
                            }
                            catch (Exception e)
                            {
                                stepResult.IsSuccessful = false;
                                stepResult.Errors = [S["Unexpected error occurred while executing the '{0}' step.", stepResult.StepName]];

                                // Because we can't do some async processing the in catch or finally
                                // blocks, we store the exception to throw it later.

                                capturedException = ExceptionDispatchInfo.Capture(new RecipeExecutionException(e, stepResult));
                            }

                            stepResult.IsCompleted = true;

                            capturedException?.Throw();

                            if (!stepResult.IsSuccessful)
                            {
                                throw new RecipeExecutionException(stepResult);
                            }

                            if (recipeStep.InnerRecipes != null)
                            {
                                foreach (var descriptor in recipeStep.InnerRecipes)
                                {
                                    var innerExecutionId = Guid.NewGuid().ToString();
                                    if (descriptor is RecipeDescriptor innerLegacy)
                                    {
                                        innerLegacy.RequireNewScope = recipeDescriptor.RequireNewScope;
                                    }
                                    await ExecuteAsync(innerExecutionId, descriptor, environment, cancellationToken);
                                }
                            }
                        }
                    }
                }
            }

            if (legacyDescriptor is not null)
            {
                await _recipeEventHandlers.InvokeAsync((handler, executionId, legacyDescriptor) => handler.RecipeExecutedAsync(executionId, legacyDescriptor), executionId, legacyDescriptor, _logger);
            }

            return executionId;
        }
        catch (Exception)
        {
            if (legacyDescriptor is not null)
            {
                await _recipeEventHandlers.InvokeAsync((handler, executionId, legacyDescriptor) => handler.ExecutionFailedAsync(executionId, legacyDescriptor), executionId, legacyDescriptor, _logger);
            }

            throw;
        }
        finally
        {
            _methodProviders.Remove(executionId);
        }
    }

    private async Task ExecuteStepAsync(RecipeExecutionContext recipeStep)
    {
        var shellScope = recipeStep.RecipeDescriptor?.RequireNewScope != false
            ? await _shellHost.GetScopeAsync(_shellSettings)
            : ShellScope.Current;

        await shellScope.UsingAsync(async scope =>
        {
            var scriptingManager = scope.ServiceProvider.GetRequiredService<IScriptingManager>();

            // Substitutes the script elements by their actual values.
            EvaluateJsonTree(scriptingManager, recipeStep, recipeStep.Step);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Executing recipe step '{RecipeName}'.", recipeStep.Name);
            }

            await _recipeEventHandlers.InvokeAsync((handler, recipeStep) => handler.RecipeStepExecutingAsync(recipeStep), recipeStep, _logger);

            // Execute legacy step handlers. This includes the RecipeDeploymentStepHandler bridge
            // which routes to IRecipeDeploymentStep implementations.
#pragma warning disable CS0618 // Type or member is obsolete
            var recipeStepHandlers = scope.ServiceProvider.GetServices<IRecipeStepHandler>();
            foreach (var recipeStepHandler in recipeStepHandlers)
            {
                await recipeStepHandler.ExecuteAsync(recipeStep);
            }
#pragma warning restore CS0618 // Type or member is obsolete

            await _recipeEventHandlers.InvokeAsync((handler, recipeStep) => handler.RecipeStepExecutedAsync(recipeStep), recipeStep, _logger);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Finished executing recipe step '{RecipeName}'.", recipeStep.Name);
            }
        });
    }

    /// <summary>
    /// Traverse all the nodes of the json document and replaces their value if they are scripted.
    /// </summary>
    private JsonNode EvaluateJsonTree(IScriptingManager scriptingManager, RecipeExecutionContext context, JsonNode node)
    {
        if (node is null)
        {
            return null;
        }

        switch (node.GetValueKind())
        {
            case JsonValueKind.Array:
                var array = node.AsArray();
                for (var i = 0; i < array.Count; i++)
                {
                    var item = EvaluateJsonTree(scriptingManager, context, array[i]);
                    if (item is JsonValue && item != array[i])
                    {
                        array[i] = item;
                    }
                }

                break;

            case JsonValueKind.Object:
                var properties = node.AsObject();
                foreach (var property in properties.ToArray())
                {
                    var newProperty = EvaluateJsonTree(scriptingManager, context, property.Value);
                    if (newProperty is JsonValue && newProperty != property.Value)
                    {
                        properties[property.Key] = newProperty;
                    }
                }

                break;

            case JsonValueKind.String:
                const char scriptSeparator = ':';
                var value = node.Value<string>();

                // Evaluate the expression while the result is another expression.
                while (value.StartsWith('[') && value.EndsWith(']'))
                {
                    var scriptSeparatorIndex = value.IndexOf(scriptSeparator);

                    // Only remove brackets if this is a valid script expression, e.g. '[js:xxx]', or '[file:xxx]'.
                    if (!(scriptSeparatorIndex > -1 && value[1..scriptSeparatorIndex].All(char.IsLetter)))
                    {
                        break;
                    }

                    value = value.Trim('[', ']');

                    // For file-based recipes (RecipeDescriptor), pass file context for [file:...] support.
                    // For code-based recipes (IRecipeDescriptor), use the simpler overload without file context.
                    var legacyDescriptor = context.RecipeDescriptor as RecipeDescriptor;
                    value = (scriptingManager.Evaluate(
                        value,
                        legacyDescriptor?.FileProvider,
                        legacyDescriptor?.BasePath,
                        _methodProviders[context.ExecutionId])
                        ?? string.Empty).ToString();
                }

                node = JsonValue.Create<string>(value);

                break;
        }

        return node;
    }
}
