using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
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
        private readonly ILogger _logger;
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;
        private readonly IRecipeStore _recipeStore;

        public RecipeExecutor(
            IEventBus eventBus,
            IRecipeStore recipeStore,
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
                RecipeResult result = new RecipeResult { ExecutionId = executionId };
                List<RecipeStepResult> stepResults = new List<RecipeStepResult>();

                using (StreamReader file = File.OpenText(recipeDescriptor.RecipeFileInfo.PhysicalPath))
                {
                    using (var reader = new JsonTextReader(file))
                    {
                        VariablesMethodProvider variablesMethodProvider = null;

                        // Go to Steps, then iterate.
                        while (reader.Read())
                        {

                            if (reader.Path == "variables")
                            {
                                reader.Read();

                                var variables = JObject.Load(reader);
                                variablesMethodProvider = new VariablesMethodProvider(variables);
                            }

                            if (reader.Path == "steps" && reader.TokenType == JsonToken.StartArray)
                            {
                                int stepId = 0;
                                while (reader.Read() && reader.Depth > 1)
                                {
                                    if (reader.Depth == 2)
                                    {
                                        var child = JToken.Load(reader);

                                        var recipeStep = new RecipeStepDescriptor
                                        {
                                            Id = (stepId++).ToString(),
                                            Name = child.Value<string>("name"),
                                            Step = child
                                        };

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
                                            var scriptingManager = scope.ServiceProvider.GetRequiredService<IScriptingManager>();

                                            if (variablesMethodProvider != null)
                                            {
                                                variablesMethodProvider.ScriptingManager = scriptingManager;
                                                scriptingManager.GlobalMethodProviders.Add(variablesMethodProvider);
                                            }

                                            if (_applicationLifetime.ApplicationStopping.IsCancellationRequested)
                                            {
                                                throw new OrchardException(T["Recipe cancelled, application is restarting"]);
                                            }

                                            EvaluateJsonTree(scriptingManager, recipeStep.Step);

                                            await recipeStepExecutor.ExecuteAsync(executionId, recipeStep);
                                        }

                                        // The recipe execution might have invalidated the shell by enabling new features,
                                        // so the deferred tasks need to run on an updated shell context if necessary.
                                        shellContext = _orchardHost.GetOrCreateShellContext(_shellSettings);
                                        using (var scope = shellContext.CreateServiceScope())
                                        {
                                            var recipeStepExecutor = scope.ServiceProvider.GetRequiredService<IRecipeStepExecutor>();

                                            var deferredTaskEngine = scope.ServiceProvider.GetService<IDeferredTaskEngine>();

                                            // The recipe might have added some deferred tasks to process
                                            if (deferredTaskEngine != null && deferredTaskEngine.HasPendingTasks)
                                            {
                                                var taskContext = new DeferredTaskContext(scope.ServiceProvider);
                                                await deferredTaskEngine.ExecuteTasksAsync(taskContext);
                                            }
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

        private void EvaluateJsonTree(IScriptingManager scriptingManager, JToken node)
        {
            var items = node as IEnumerable<JToken>;

            switch (node.Type)
            {
                case JTokenType.Array:
                    foreach (var token in (JArray)node)
                    {
                        EvaluateJsonTree(scriptingManager, token);
                    }
                    break;
                case JTokenType.Object:
                    foreach (var property in (JObject)node)
                    {
                        if (property.Value.Type == JTokenType.String)
                        {
                            var value = property.Value.Value<string>();

                            // Evaluate the expression while the result is another expression
                            while (value.StartsWith("[") && value.EndsWith("]"))
                            {
                                value = value.Trim('[', ']');
                                value = (scriptingManager.Evaluate(value) ?? "").ToString();
                                node[property.Key] = new JValue(value);
                            }
                        }
                        else
                        {
                            EvaluateJsonTree(scriptingManager, property.Value);
                        }
                    }
                    break;
            }
        }
    }
}