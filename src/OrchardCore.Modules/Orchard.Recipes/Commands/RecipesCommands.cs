using Microsoft.Extensions.Localization;
using Orchard.Environment.Commands;
using Orchard.Recipes.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Recipes.Commands
{
    public class RecipesCommands : DefaultCommandHandler
    {
        private readonly IRecipeHarvester _recipeHarvester;

        public RecipesCommands(
            IRecipeHarvester recipeHarvester,
            IStringLocalizer<RecipesCommands> localizer) : base(localizer)
        {
            _recipeHarvester = recipeHarvester;
        }

        [CommandHelp("recipes harvest <extensionId>", "\tDisplays a list of available recipes for a specific extension.")]
        [CommandName("recipes harvest")]
        public async Task Harvest(string extensionId)
        {
            var recipes = await _recipeHarvester.HarvestRecipesAsync(extensionId);
            if (!recipes.Any())
            {
                await Context.Output.WriteLineAsync(T[$"No recipes found for extension '{extensionId}'."]);
                return;
            }

            await Context.Output.WriteLineAsync(T["List of available recipes"]);
            await Context.Output.WriteLineAsync(T["--------------------------"]);
            await Context.Output.WriteLineAsync();

            foreach (var recipe in recipes)
            {
                await Context.Output.WriteLineAsync(T[$"Recipe: {recipe.Name}"]);
                await Context.Output.WriteLineAsync(T[$"  Version:     {recipe.Version}"]);
                await Context.Output.WriteLineAsync(T[$"  Tags:        {string.Join(",", recipe.Tags)}"]);
                await Context.Output.WriteLineAsync(T[$"  Description: {recipe.Description}"]);
                await Context.Output.WriteLineAsync(T[$"  Author:      {recipe.Author}"]);
                await Context.Output.WriteLineAsync(T[$"  Website:     {recipe.WebSite}"]);
            }
        }
    }
}
