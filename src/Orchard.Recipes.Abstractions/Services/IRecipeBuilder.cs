using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Orchard.Recipes.Services
{
    public interface IRecipeBuilder
    {
        JObject Build(IEnumerable<IRecipeBuilderStep> steps);
    }
}