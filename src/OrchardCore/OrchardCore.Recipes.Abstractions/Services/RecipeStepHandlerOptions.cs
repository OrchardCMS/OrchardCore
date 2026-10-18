namespace OrchardCore.Recipes.Services;

/// <summary>
/// The recipe step handler types, by the name of the step they process.
/// </summary>
/// <remarks>
/// Handlers registered with <see cref="ServiceCollectionExtensions.AddRecipeExecutionStep{TImplementation}(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>
/// that declare a <see cref="RecipeStepAttribute"/> are added here, so that only the handlers of an executed step are created.
/// </remarks>
public sealed class RecipeStepHandlerOptions
{
    private readonly Dictionary<string, List<Type>> _handlerTypes = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds a handler type for a recipe step. Adding the same handler type twice for a step has no effect.
    /// </summary>
    /// <param name="stepName">The name of the recipe step, matched case-insensitively.</param>
    /// <param name="handlerType">The handler type. It must implement <see cref="IRecipeStepHandler"/>.</param>
    public RecipeStepHandlerOptions AddHandler(string stepName, Type handlerType)
    {
        ArgumentException.ThrowIfNullOrEmpty(stepName);
        ArgumentNullException.ThrowIfNull(handlerType);

        if (!typeof(IRecipeStepHandler).IsAssignableFrom(handlerType))
        {
            throw new ArgumentException($"The type must implement the '{nameof(IRecipeStepHandler)}' interface.", nameof(handlerType));
        }

        if (!_handlerTypes.TryGetValue(stepName, out var handlerTypes))
        {
            _handlerTypes[stepName] = handlerTypes = [];
        }

        if (!handlerTypes.Contains(handlerType))
        {
            handlerTypes.Add(handlerType);
        }

        return this;
    }

    /// <summary>
    /// Gets the handler types of a recipe step, in registration order.
    /// </summary>
    /// <param name="stepName">The name of the recipe step, matched case-insensitively.</param>
    public IReadOnlyList<Type> GetHandlerTypes(string stepName)
        => stepName is not null && _handlerTypes.TryGetValue(stepName, out var handlerTypes)
            ? handlerTypes
            : [];
}
