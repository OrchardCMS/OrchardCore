using Orchard.Recipes.Models;
using System.Linq;
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

        public RecipeResult GetResult(string executionId)
        {
            var stepResults = _session
                .QueryAsync<RecipeResult>()
                .List()
                .Result;

            return stepResults.SingleOrDefault(x => x.ExecutionId == executionId);
        }
    }
}
