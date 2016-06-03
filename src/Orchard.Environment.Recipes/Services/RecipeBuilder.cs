using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Services;

namespace Orchard.Environment.Recipes.Services
{
    public class RecipeBuilder : IRecipeBuilder
    {
        private readonly IClock _clock;

        public RecipeBuilder(IClock clock)
        {
            _clock = clock;
        }

        public XDocument Build(IEnumerable<IRecipeBuilderStep> steps)
        {
            var context = new BuildContext
            {
                RecipeDocument = CreateRecipeRoot()
            };

            foreach (var step in steps.OrderByDescending(x => x.Priority))
            {
                step.Build(context);
            }

            return context.RecipeDocument;
        }

        private XDocument CreateRecipeRoot()
        {
            var recipeRoot = new XDocument(
                new XDeclaration("1.0", "", "yes"),
                new XComment("Exported from Orchard"),
                new XElement("Orchard",
                    new XElement("Recipe",
                        new XElement("ExportUtc", _clock.UtcNow))
                )
            );
            return recipeRoot;
        }
    }
}