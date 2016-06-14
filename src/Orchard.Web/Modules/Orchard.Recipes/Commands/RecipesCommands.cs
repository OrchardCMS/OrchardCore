using Microsoft.Extensions.Localization;
using Orchard.Environment.Commands;
using Orchard.Environment.Extensions;
using Orchard.Recipes.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Recipes.Commands
{
    public class RecipesCommands : DefaultOrchardCommandHandler
    {
        private readonly IRecipeHarvester _recipeHarvester;
        private readonly IExtensionManager _extensionManager;

        public RecipesCommands(
            IRecipeHarvester recipeHarvester,
            IExtensionManager extensionManager,
            IStringLocalizer<RecipesCommands> localizer) : base(localizer)
        {
            _recipeHarvester = recipeHarvester;
            _extensionManager = extensionManager;
        }

        [CommandHelp("recipes harvest <extensionId>\r\n\t" + "Displays a list of available recipes for a specific extension.")]
        [CommandName("recipes harvest")]
        public async Task Harvest(string extensionId)
        {
            var recipes = await _recipeHarvester.HarvestRecipesAsync(extensionId);
            if (!recipes.Any())
            {
                Context.Output.WriteLine(T["No recipes found for extension '{0}'.", extensionId]);
                return;
            }

            Context.Output.WriteLine(T["List of available recipes"]);
            Context.Output.WriteLine(T["--------------------------"]);
            Context.Output.WriteLine();

            foreach (var recipe in recipes)
            {
                Context.Output.WriteLine(T["Recipe: {0}", recipe.Name]);
                Context.Output.WriteLine(T["  Version:     {0}", recipe.Version]);
                Context.Output.WriteLine(T["  Tags:        {0}", recipe.Tags]);
                Context.Output.WriteLine(T["  Description: {0}", recipe.Description]);
                Context.Output.WriteLine(T["  Author:      {0}", recipe.Author]);
                Context.Output.WriteLine(T["  Website:     {0}", recipe.WebSite]);
            }
        }
    }
}
