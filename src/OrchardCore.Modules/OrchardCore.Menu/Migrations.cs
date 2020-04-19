using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Menu
{
    public class Migrations : DataMigration
    {
        private readonly IRecipeMigrator _recipeMigrator;

        public Migrations(IRecipeMigrator recipeMigrator)
        {
            _recipeMigrator = recipeMigrator;
        }

        public async Task<int> CreateAsync()
        {
            // Return 1 to shortcut the second migration on new content definition schemas.
            return await Task.FromResult(1);
        }

        public async Task<int> UpdateFrom1Async()
        {
            await _recipeMigrator.ExecuteAsync("menu.recipe.json", this);

            return 2;
        }
    }
}
