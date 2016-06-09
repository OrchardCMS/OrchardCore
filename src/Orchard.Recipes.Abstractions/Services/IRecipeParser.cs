using System.Xml.Linq;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public interface IRecipeParser
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