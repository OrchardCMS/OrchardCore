using System.Threading.Tasks;
using OrchardCore.Recipes.Models;
using YesSql;

namespace OrchardCore.Recipes.Services
{
    /// <summary>
    /// And implementation of <see cref="IRecipeStore"/> that stores the recipe<see cref="RecipeResult"/>
    /// object in YesSql.
    /// </summary>
    public class RecipeStore : IRecipeStore
    {
        private readonly ISession _session;

        public RecipeStore(ISession session)
        {
            _session = session;
        }

        public Task CreateAsync(RecipeResult recipeResult)
        {
            _session.Save(recipeResult);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(RecipeResult recipeResult)
        {
            _session.Delete(recipeResult);
            return Task.CompletedTask;
        }

        public Task<RecipeResult> FindByExecutionIdAsync(string executionId)
        {
            return _session.Query<RecipeResult, RecipeResultIndex>(x => x.ExecutionId == executionId).FirstOrDefaultAsync();
        }

        public Task UpdateAsync(RecipeResult recipeResult)
        {
            _session.Save(recipeResult);
            return Task.CompletedTask;
        }
    }
}
