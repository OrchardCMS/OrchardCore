
namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// Describes one function formulas can call.
/// </summary>
public sealed class ExpressionFunction
{
    /// <summary>
    /// Gets or sets the upper-case function name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets or sets the group the function is listed under in the designer.
    /// </summary>
    public string Category { get; init; }

    /// <summary>
    /// Gets or sets how the function is written, such as <c>LEFT(text, count)</c>.
    /// </summary>
    public string Signature { get; init; }

    /// <summary>
    /// Gets or sets the English description of the function, localized by the designer.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Gets or sets the fewest arguments the function takes.
    /// </summary>
    public int MinArguments { get; init; }

    /// <summary>
    /// Gets or sets the most arguments the function takes, or <see cref="int.MaxValue"/> for any number.
    /// </summary>
    public int MaxArguments { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the function aggregates its argument over the rows of a group.
    /// </summary>
    public bool IsAggregate { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the argument of an aggregate function must be a number.
    /// </summary>
    public bool RequiresNumericArgument { get; init; }

    /// <summary>
    /// Gets or sets the function that infers the result type from the argument types.
    /// </summary>
    public Func<IReadOnlyList<DataFieldType>, DataFieldType> ReturnType { get; init; }

    /// <summary>
    /// Gets or sets the implementation of a scalar function, called with the evaluated arguments.
    /// </summary>
    public Func<object[], ExpressionContext, object> Invoke { get; init; }

    /// <summary>
    /// Gets or sets the implementation of an aggregate function, called with the argument evaluated for every row of
    /// the group.
    /// </summary>
    public Func<IReadOnlyList<object>, object> Aggregate { get; init; }
}
