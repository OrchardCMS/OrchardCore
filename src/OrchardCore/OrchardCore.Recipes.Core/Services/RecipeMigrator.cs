using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Extensions;
using OrchardCore.Modules;

namespace OrchardCore.Recipes.Services
{
    public class RecipeMigrator : IRecipeMigrator
    {
        private readonly IRecipeReader _recipeReader;
        private readonly IRecipeExecutor _recipeExecutor;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly IEnumerable<IRecipeEnvironmentProvider> _environmentProviders;
        private readonly ILogger _logger;

        public RecipeMigrator(
            IRecipeReader recipeReader,
            IRecipeExecutor recipeExecutor,
            IHostEnvironment hostingEnvironment,
            ITypeFeatureProvider typeFeatureProvider,
            IEnumerable<IRecipeEnvironmentProvider> environmentProviders,
            ILogger<RecipeMigrator> logger)
        {
            _recipeReader = recipeReader;
            _recipeExecutor = recipeExecutor;
            _hostingEnvironment = hostingEnvironment;
            _typeFeatureProvider = typeFeatureProvider;
            _environmentProviders = environmentProviders;
            _logger = logger;
        }

        public async Task<string> ExecuteAsync(string recipeFileName, IDataMigration migration)
        {
            var featureInfo = _typeFeatureProvider.GetFeatureForDependency(migration.GetType());

            var recipeBasePath = Path.Combine(featureInfo.Extension.SubPath, "Migrations").Replace('\\', '/');
            var recipeFilePath = Path.Combine(recipeBasePath, recipeFileName).Replace('\\', '/');
            var recipeFileInfo = _hostingEnvironment.ContentRootFileProvider.GetFileInfo(recipeFilePath);
            var recipeDescriptor = await _recipeReader.GetRecipeDescriptorAsync(recipeBasePath, recipeFileInfo, _hostingEnvironment.ContentRootFileProvider);
            recipeDescriptor.RequireNewScope = false;

            var environment = new Dictionary<string, object>();

            await _environmentProviders.OrderBy(x => x.Order).InvokeAsync((provider, env) => provider.PopulateEnvironmentAsync(env), environment, _logger);

            var executionId = Guid.NewGuid().ToString("n");
            return await _recipeExecutor.ExecuteAsync(executionId, recipeDescriptor, environment, CancellationToken.None);
        }
    }
}
