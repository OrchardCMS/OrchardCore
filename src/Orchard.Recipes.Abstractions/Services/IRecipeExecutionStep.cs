using System.Threading.Tasks;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    /// <summary>
    /// An implementation of this interface will be used everytime a recipe step is processed.
    /// Each implementation is reponsible for processing only the steps that it targets.
    /// </summary>
    public interface IRecipeStepHandler
    {
        /// <summary>
        /// Processes a recipe step.
        /// </summary>
        /// <param name="context">The context object representing the processed step.</param>
        Task ExecuteAsync(RecipeExecutionContext context);
    }
}