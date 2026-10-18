namespace OrchardCore.Recipes.Services;

/// <summary>
/// Declares the name of the recipe step that a <see cref="NamedRecipeStepHandler"/> processes. Every concrete
/// <see cref="NamedRecipeStepHandler"/> must declare it.
/// </summary>
/// <remarks>
/// The name describes the handler type, not its instances, so the recipe executor only creates the handlers of the
/// steps it executes.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RecipeStepAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeStepAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the recipe step, matched case-insensitively.</param>
    public RecipeStepAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
    }

    /// <summary>
    /// Gets the name of the recipe step, matched case-insensitively.
    /// </summary>
    public string Name { get; }
}
