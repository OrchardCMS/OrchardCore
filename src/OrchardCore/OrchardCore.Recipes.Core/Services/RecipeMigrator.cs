using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Recipes.Services
{
    public class RecipeMigrator : IRecipeMigrator
    {
        private readonly IRecipeReader _recipeReader;
        private readonly IRecipeExecutor _recipeExecutor;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        public RecipeMigrator(
            IRecipeReader recipeReader,
            IRecipeExecutor recipeExecutor,
            IHostEnvironment hostingEnvironment,
            ITypeFeatureProvider typeFeatureProvider)
        {
            _recipeReader = recipeReader;
            _recipeExecutor = recipeExecutor;
            _hostingEnvironment = hostingEnvironment;
            _typeFeatureProvider = typeFeatureProvider;
        }

        public async Task<string> ExecuteAsync(string recipeFileName, IDataMigration migration)
        {
            var featureInfo = _typeFeatureProvider.GetFeatureForDependency(migration.GetType());

            var recipeBasePath = Path.Combine(featureInfo.Extension.SubPath, "Migrations").Replace('\\', '/');
            var recipeFilePath = Path.Combine(recipeBasePath, recipeFileName).Replace('\\', '/');
            var recipeFileInfo = _hostingEnvironment.ContentRootFileProvider.GetFileInfo(recipeFilePath);
            var recipeDescriptor = await _recipeReader.GetRecipeDescriptor(recipeBasePath, recipeFileInfo, _hostingEnvironment.ContentRootFileProvider);
            recipeDescriptor.RequireNewScope = false;

            var executionId = Guid.NewGuid().ToString("n");
            return await _recipeExecutor.ExecuteAsync(executionId, recipeDescriptor, new object(), CancellationToken.None);
        }
    }
}
