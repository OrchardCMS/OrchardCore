using Orchard.Recipes.Models;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Recipes.Services
{
    public class RecipeResultAccessor : IRecipeResultAccessor
    {
        private readonly ISession _session;
        public RecipeResultAccessor(ISession session)
        {
            _session = session;
        }

        public async Task<RecipeResult> GetResultAsync(string executionId)
        {
            return await _session
                .QueryAsync<RecipeResult, RecipeResultIndex>(x => x.ExecutionId == executionId)
                .FirstOrDefault();
        }
    }
}
