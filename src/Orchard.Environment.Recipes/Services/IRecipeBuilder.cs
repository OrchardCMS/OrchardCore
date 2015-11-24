using Orchard.DependencyInjection;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeBuilder : IDependency
    {
        XDocument Build(IEnumerable<IRecipeBuilderStep> steps);
    }
}