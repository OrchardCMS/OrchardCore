namespace OrchardCore.Recipes.Services;

public interface IRecipeExecutor
{
    Task<string> ExecuteAsync(string executionId, IRecipeDescriptor recipeDescriptor, IDictionary<string, object> environment, CancellationToken cancellationToken);
}
