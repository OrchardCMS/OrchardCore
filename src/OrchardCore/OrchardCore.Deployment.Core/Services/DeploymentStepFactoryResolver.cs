using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Deployment.Core.Services;

/// <summary>
/// Resolves all deployment step factories by combining explicitly registered factories
/// with auto-discovered factories from exportable recipe steps.
/// </summary>
/// <remarks>
/// Recipe steps that extend <see cref="RecipeDeploymentStep{TModel}"/> (not <see cref="RecipeImportStep{TModel}"/>)
/// automatically get a corresponding deployment step factory without requiring explicit registration.
/// </remarks>
public sealed class DeploymentStepFactoryResolver : IDeploymentStepFactoryResolver
{
    private readonly IEnumerable<IDeploymentStepFactory> _explicitFactories;
    private readonly IServiceScopeFactory _scopeFactory;
    private IReadOnlyList<IDeploymentStepFactory> _allFactories;
    private readonly object _lock = new();

    public DeploymentStepFactoryResolver(
        IEnumerable<IDeploymentStepFactory> explicitFactories,
        IServiceScopeFactory scopeFactory)
    {
        _explicitFactories = explicitFactories;
        _scopeFactory = scopeFactory;
    }

    public IReadOnlyList<IDeploymentStepFactory> GetFactories()
    {
        if (_allFactories is not null)
        {
            return _allFactories;
        }

        lock (_lock)
        {
            if (_allFactories is not null)
            {
                return _allFactories;
            }

            var factories = new List<IDeploymentStepFactory>(_explicitFactories);
            var existingNames = new HashSet<string>(
                factories.Select(f => f.Name), StringComparer.OrdinalIgnoreCase);

            using var scope = _scopeFactory.CreateScope();
            var recipeSteps = scope.ServiceProvider.GetServices<IRecipeDeploymentStep>();

            foreach (var step in recipeSteps)
            {
                if (!SupportsExport(step.GetType()))
                {
                    continue;
                }

                var factoryName = $"{nameof(RecipeStepDeploymentStep)}_{step.Name}";
                if (existingNames.Contains(factoryName))
                {
                    continue;
                }

                factories.Add(new RecipeStepDeploymentStepFactory(step.Name));
                existingNames.Add(factoryName);
            }

            _allFactories = factories;
        }

        return _allFactories;
    }

    public IDeploymentStepFactory GetFactory(string name)
    {
        return GetFactories().FirstOrDefault(f =>
            string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether a recipe step type supports export by checking that it does NOT
    /// extend <see cref="RecipeImportStep{TModel}"/> (which seals export to return null).
    /// </summary>
    private static bool SupportsExport(Type type)
    {
        var current = type;
        while (current != null)
        {
            if (current.IsGenericType &&
                current.GetGenericTypeDefinition() == typeof(RecipeImportStep<>))
            {
                return false;
            }

            current = current.BaseType;
        }

        return true;
    }
}
