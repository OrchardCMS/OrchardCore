using System.Collections.Generic;
using System.Xml.Linq;

namespace Orchard.Recipes.Services
{
    public interface IRecipeBuilder
    {
        XDocument Build(IEnumerable<IRecipeBuilderStep> steps);
    }
}