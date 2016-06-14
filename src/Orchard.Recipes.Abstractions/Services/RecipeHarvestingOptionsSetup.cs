using Microsoft.Extensions.Options;

namespace Orchard.Recipes.Services
{
    /// <summary>
    /// Sets up default options for <see cref="RecipeHarvestingOptions"/>.
    /// </summary>
    public class RecipeHarvestingOptionsSetup : ConfigureOptions<RecipeHarvestingOptions>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RecipeHarvestingOptions"/>.
        /// </summary>
        public RecipeHarvestingOptionsSetup()
            : base(options => { })
        {
        }
    }
}