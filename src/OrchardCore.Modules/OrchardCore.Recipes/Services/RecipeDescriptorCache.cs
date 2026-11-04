using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Keeps the harvested recipes of a tenant, as harvesting parses every recipe file of every extension. This
/// singleton lives in the tenant container, so releasing the tenant's shell is what invalidates it.
/// </summary>
internal sealed class RecipeDescriptorCache
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private RecipeDescriptor[] _recipes;

    public async Task<RecipeDescriptor[]> GetAsync(IEnumerable<IRecipeHarvester> harvesters)
    {
        if (_recipes is not null)
        {
            return _recipes;
        }

        await _lock.WaitAsync();

        try
        {
            if (_recipes is null)
            {
                var collections = await Task.WhenAll(harvesters.Select(harvester => harvester.HarvestRecipesAsync()));
                _recipes = collections.SelectMany(collection => collection).ToArray();
            }

            return _recipes;
        }
        finally
        {
            _lock.Release();
        }
    }
}
