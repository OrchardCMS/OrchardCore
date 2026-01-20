using System.Text.Json.Nodes;
using OrchardCore.Search.Models;

namespace OrchardCore.Recipes.Models;

public static class RecipeExecutionContextExtensions
{
    /// <summary>
    /// Converts the <paramref name="context"/> into <see cref="IndexRecipeStep{T}"/> and returns all the indexes with
    /// the name property correctly set.
    /// </summary>
    public static IEnumerable<T> GetIndexSettings<T>(this RecipeExecutionContext context)
        where T : IndexSettingsBase =>
        context
            .Step
            .ToObject<IndexRecipeStep<T>>()
            .Indexes
            .SelectMany(indexes => indexes.Select(pair =>
            {
                pair.Value.IndexName = pair.Key;
                return pair.Value;
            }));
}
