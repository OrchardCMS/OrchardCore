namespace OrchardCore.Recipes.Services;

public interface IRecipeEnvironmentProvider
{
    Task PopulateEnvironmentAsync(IDictionary<string, object> environment);
    int Order { get; }
}
