using System.Text.Json.Nodes;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Base implementation of <see cref="IRecipeStepDescriptor"/> that provides common functionality
/// for recipe step descriptors.
/// </summary>
public abstract class RecipeStepDescriptor : IRecipeStepDescriptor
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract string DisplayName { get; }

    /// <inheritdoc />
    public abstract string Description { get; }

    /// <inheritdoc />
    public virtual string Category => "General";

    /// <inheritdoc />
    public virtual ValueTask<JsonObject> GetSchemaAsync()
        => ValueTask.FromResult<JsonObject>(null);
}
