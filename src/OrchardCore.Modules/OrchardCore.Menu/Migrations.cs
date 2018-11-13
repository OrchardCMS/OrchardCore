using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Menu
{
    public class Migrations : DataMigration
    {
        private readonly IRecipeReader _recipeReader;
        private readonly IRecipeExecutor _recipeExecutor;
        private readonly IHostingEnvironment _hostingEnvironment;

        public Migrations(
            IRecipeReader recipeReader,
            IRecipeExecutor recipeExecutor,
            IHostingEnvironment hostingEnvironment)
        {
            _recipeReader = recipeReader;
            _recipeExecutor = recipeExecutor;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<int> CreateAsync()
        {
            var recipeDescriptor = await _recipeReader.GetRecipeDescriptor(
                "Areas/OrchardCore.Menu/Migrations/menu.recipe.json",
                _hostingEnvironment.ContentRootFileProvider);

            var executionId = Guid.NewGuid().ToString("n");

            await _recipeExecutor.ExecuteMigrationAsync(executionId, recipeDescriptor, new object());

            return 1;
        }
    }
}
