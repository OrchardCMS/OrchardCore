
namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// Describes one function formulas can call. Offer a function to every formula with
/// <c>services.AddExpressionFunction(...)</c>, or several with an <see cref="IExpressionFunctionProvider"/>.
/// </summary>
public sealed class ExpressionFunction
{
    /// <summary>
    /// Gets or sets the function name, such as <c>UPPER</c>. Formulas call it ignoring case. It starts with a letter
    /// or an underscore, holds only letters, digits and underscores, and isn't a keyword such as <c>AND</c>.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets or sets the group the function is listed under in the list of functions, such as <c>Text</c>.
    /// </summary>
    public string Category { get; init; }

    /// <summary>
    /// Gets or sets how the function is written, such as <c>LEFT(text, count)</c>.
    /// </summary>
    public string Signature { get; init; }

    /// <summary>
    /// Gets or sets the English description of the function, which the list of functions passes through the
    /// localizer.
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

    /// <summary>
    /// Creates a function that computes a value from the values of its arguments, for one row at a time. Return a
    /// missing value (<see langword="null"/>) rather than throwing when an argument is missing or of the wrong type.
    /// </summary>
    /// <param name="name">The function name, such as <c>TAX</c>.</param>
    /// <param name="category">The group the function is listed under, such as <c>Number</c>.</param>
    /// <param name="signature">How the function is written, such as <c>TAX(amount, [rate])</c>.</param>
    /// <param name="description">The English description of the function.</param>
    /// <param name="minArguments">The fewest arguments the function takes.</param>
    /// <param name="maxArguments">The most arguments the function takes, or <see cref="int.MaxValue"/> for any number.</param>
    /// <param name="returnType">Infers the type of the result from the types of the arguments.</param>
    /// <param name="invoke">Computes the result from the values of the arguments.</param>
    /// <returns>The function.</returns>
    public static ExpressionFunction CreateScalar(
        string name,
        string category,
        string signature,
        string description,
        int minArguments,
        int maxArguments,
        Func<IReadOnlyList<DataFieldType>, DataFieldType> returnType,
        Func<object[], object> invoke)
    {
        ArgumentNullException.ThrowIfNull(returnType);
        ArgumentNullException.ThrowIfNull(invoke);

        return new ExpressionFunction
        {
            Name = name,
            Category = category,
            Signature = signature,
            Description = description,
            MinArguments = minArguments,
            MaxArguments = maxArguments,
            ReturnType = returnType,
            Invoke = (args, _) => invoke(args),
        };
    }

    /// <summary>
    /// Creates a function that aggregates its argument over the rows of a group, listed under the
    /// <see cref="ExpressionFunctions.AggregateCategory"/> category.
    /// </summary>
    /// <param name="name">The function name, such as <c>PRODUCT</c>.</param>
    /// <param name="signature">How the function is written, such as <c>PRODUCT(number)</c>.</param>
    /// <param name="description">The English description of the function.</param>
    /// <param name="requiresNumericArgument">Whether the argument must be a number.</param>
    /// <param name="returnType">Infers the type of the result from the type of the argument.</param>
    /// <param name="aggregate">Computes the result from the value of the argument for every row of the group.</param>
    /// <returns>The function.</returns>
    public static ExpressionFunction CreateAggregate(
        string name,
        string signature,
        string description,
        bool requiresNumericArgument,
        Func<IReadOnlyList<DataFieldType>, DataFieldType> returnType,
        Func<IReadOnlyList<object>, object> aggregate)
    {
        ArgumentNullException.ThrowIfNull(returnType);
        ArgumentNullException.ThrowIfNull(aggregate);

        return new ExpressionFunction
        {
            Name = name,
            Category = ExpressionFunctions.AggregateCategory,
            Signature = signature,
            Description = description,
            MinArguments = 1,
            MaxArguments = 1,
            IsAggregate = true,
            RequiresNumericArgument = requiresNumericArgument,
            ReturnType = returnType,
            Aggregate = aggregate,
        };
    }
}
