
namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// A formula compiled against a scope: its result type, whether it aggregates, the fields it reads, the aggregates it
/// computes, and the delegate that evaluates it.
/// </summary>
public sealed class CompiledExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompiledExpression"/> class.
    /// </summary>
    /// <param name="dataType">The type of the values the formula returns.</param>
    /// <param name="isAggregate">Whether the formula aggregates a group of rows.</param>
    /// <param name="references">The keys of the fields the formula reads.</param>
    /// <param name="evaluate">The delegate that evaluates the formula.</param>
    public CompiledExpression(
        DataFieldType dataType,
        bool isAggregate,
        IReadOnlySet<string> references,
        Func<ExpressionContext, object> evaluate)
        : this(dataType, isAggregate, references, evaluate, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompiledExpression"/> class.
    /// </summary>
    /// <param name="dataType">The type of the values the formula returns.</param>
    /// <param name="isAggregate">Whether the formula aggregates a group of rows.</param>
    /// <param name="references">The keys of the fields the formula reads.</param>
    /// <param name="evaluate">The delegate that evaluates the formula.</param>
    /// <param name="aggregates">The aggregate calls of an aggregate formula, in the order it reads their results from
    /// <see cref="ExpressionContext.AggregateValues"/>.</param>
    public CompiledExpression(
        DataFieldType dataType,
        bool isAggregate,
        IReadOnlySet<string> references,
        Func<ExpressionContext, object> evaluate,
        IReadOnlyList<ExpressionAggregate> aggregates)
    {
        ArgumentNullException.ThrowIfNull(aggregates);

        DataType = dataType;
        IsAggregate = isAggregate;
        References = references;
        Evaluate = evaluate;
        Aggregates = aggregates;
    }

    /// <summary>
    /// Gets the type of the values the formula returns.
    /// </summary>
    public DataFieldType DataType { get; }

    /// <summary>
    /// Gets a value indicating whether the formula aggregates a group of rows, and so must be evaluated with
    /// <see cref="ExpressionContext.Group"/> or <see cref="ExpressionContext.AggregateValues"/> set.
    /// </summary>
    public bool IsAggregate { get; }

    /// <summary>
    /// Gets the aggregate calls of an aggregate formula, such as the <c>SUM</c> and the <c>COUNT</c> of
    /// <c>SUM([Amount]) / COUNT()</c>; empty for a formula that reads one row. To compute the formula as the rows of
    /// its groups arrive, such as with <see cref="Operations.DataAggregator"/>, evaluate the
    /// <see cref="ExpressionAggregate.Argument"/> of each call for every row, aggregate the values with its
    /// <see cref="ExpressionAggregate.Function"/>, and evaluate the formula with the results in
    /// <see cref="ExpressionContext.AggregateValues"/>, in this order.
    /// </summary>
    public IReadOnlyList<ExpressionAggregate> Aggregates { get; }

    /// <summary>
    /// Gets a value indicating whether every aggregate call of an aggregate formula names its
    /// <see cref="ExpressionAggregate.Function"/>, so the formula can be computed as the rows arrive. A call of an
    /// aggregate function that needs all the values of its group at once can only be computed over
    /// <see cref="ExpressionContext.Group"/>.
    /// </summary>
    public bool CanAggregateAsRowsArrive => IsAggregate && Aggregates.All(aggregate => aggregate.Function != DataAggregate.None);

    /// <summary>
    /// Gets the keys of the fields the formula reads, including those read through other calculated fields.
    /// </summary>
    public IReadOnlySet<string> References { get; }

    /// <summary>
    /// Gets the delegate that evaluates the formula.
    /// </summary>
    public Func<ExpressionContext, object> Evaluate { get; }
}
