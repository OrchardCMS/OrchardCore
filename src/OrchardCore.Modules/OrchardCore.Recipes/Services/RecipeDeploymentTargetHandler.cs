using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using OrchardCore.Deployment;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services
{
    public class RecipeDeploymentTargetHandler : IDeploymentTargetHandler
    {
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IRecipeExecutor _recipeExecutor;
        private readonly IEnumerable<IRecipeEnvironmentProvider> _environmentProviders;
        private readonly ILogger _logger;

        public RecipeDeploymentTargetHandler(IShellHost shellHost,
            ShellSettings shellSettings,
            IRecipeExecutor recipeExecutor,
            IEnumerable<IRecipeEnvironmentProvider> environmentProviders,
            ILogger<RecipeDeploymentTargetHandler> logger)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _recipeExecutor = recipeExecutor;
            _environmentProviders = environmentProviders;
            _logger = logger;
        }

        public async Task ImportFromFileAsync(IFileProvider fileProvider)
        {
            var executionId = Guid.NewGuid().ToString("n");
            var recipeDescriptor = new RecipeDescriptor
            {
                FileProvider = fileProvider,
                BasePath = "",
                RecipeFileInfo = fileProvider.GetFileInfo("Recipe.json")
            };

            var environment = new Dictionary<string, object>();
            await _environmentProviders.OrderBy(x => x.Order).InvokeAsync((provider, env) => provider.PopulateEnvironmentAsync(env), environment, _logger);

            await _recipeExecutor.ExecuteAsync(executionId, recipeDescriptor, environment, CancellationToken.None);

            await _shellHost.ReleaseShellContextAsync(_shellSettings);
        }
    }
}
