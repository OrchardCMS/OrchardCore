using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Deployment;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services
{
    public class RecipeDeploymentTargetHandler : IDeploymentTargetHandler
    {
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IRecipeExecutor _recipeExecutor;

        public RecipeDeploymentTargetHandler(IShellHost shellHost, ShellSettings shellSettings, IRecipeExecutor recipeExecutor)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _recipeExecutor = recipeExecutor;
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

            await _recipeExecutor.ExecuteAsync(executionId, recipeDescriptor, new object(), CancellationToken.None);

            await _shellHost.ReleaseShellContextAsync(_shellSettings);
        }
    }
}
