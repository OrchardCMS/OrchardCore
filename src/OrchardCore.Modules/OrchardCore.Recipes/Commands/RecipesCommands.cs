using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Commands;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.Commands;

public class RecipesCommands : DefaultCommandHandler
{
    private readonly IEnumerable<IRecipeHarvester> _recipeHarvesters;

    public RecipesCommands(
        IEnumerable<IRecipeHarvester> recipeHarvesters,
        IStringLocalizer<RecipesCommands> localizer) : base(localizer)
    {
        _recipeHarvesters = recipeHarvesters;
    }

    [CommandHelp("recipes harvest", "\tDisplays a list of available recipes.")]
    [CommandName("recipes harvest")]
    public async Task Harvest()
    {
        var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync())).ConfigureAwait(false);
        var recipes = recipeCollections.SelectMany(x => x).ToArray();

        if (recipes.Length == 0)
        {
            await Context.Output.WriteLineAsync(S["No recipes found."]).ConfigureAwait(false);
            return;
        }

        await Context.Output.WriteLineAsync(S["List of available recipes"]).ConfigureAwait(false);
        await Context.Output.WriteLineAsync("--------------------------").ConfigureAwait(false);
        await Context.Output.WriteLineAsync().ConfigureAwait(false);

        foreach (var recipe in recipes)
        {
            await Context.Output.WriteLineAsync(S["Recipe: {0}", recipe.Name]).ConfigureAwait(false);
            await Context.Output.WriteLineAsync(S["  Version:     {0}", recipe.Version]).ConfigureAwait(false);
            await Context.Output.WriteLineAsync(S["  Tags:        {0}", string.Join(",", recipe.Tags)]).ConfigureAwait(false);
            await Context.Output.WriteLineAsync(S["  Description: {0}", recipe.Description]).ConfigureAwait(false);
            await Context.Output.WriteLineAsync(S["  Author:      {0}", recipe.Author]).ConfigureAwait(false);
            await Context.Output.WriteLineAsync(S["  Website:     {0}", recipe.WebSite]).ConfigureAwait(false);
        }
    }
}
