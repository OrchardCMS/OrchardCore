using System.Xml.Linq;
using Orchard.Environment.Recipes.Models;
using Orchard.DependencyInjection;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeParser : IDependency
    {
        Recipe ParseRecipe(XDocument recipeDocument);
    }

    public static class RecipeParserExtensions
    {
        public static Recipe ParseRecipe(this IRecipeParser recipeParser, string recipeText)
        {
            var recipeDocument = XDocument.Parse(recipeText, LoadOptions.PreserveWhitespace);
            return recipeParser.ParseRecipe(recipeDocument);
        }
    }
}