namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// One aggregate call of an aggregate formula, such as the <c>SUM([Amount])</c> of <c>SUM([Amount]) / COUNT()</c>: what it
/// aggregates and how. See <see cref="CompiledExpression.Aggregates"/>.
/// </summary>
public sealed class ExpressionAggregate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionAggregate"/> class.
    /// </summary>
    /// <param name="function">The aggregate the call computes, or <see cref="DataAggregate.None"/> for a function that
    /// needs all the values of the group at once.</param>
    /// <param name="argument">Evaluates the argument of the call for <see cref="ExpressionContext.Row"/>.</param>
    public ExpressionAggregate(DataAggregate function, Func<ExpressionContext, object> argument)
    {
        ArgumentNullException.ThrowIfNull(argument);

        Function = function;
        Argument = argument;
    }

    /// <summary>
    /// Gets the aggregate the call computes, or <see cref="DataAggregate.None"/> for a function that needs all the values
    /// of the group at once.
    /// </summary>
    public DataAggregate Function { get; }

    /// <summary>
    /// Gets the delegate that evaluates the argument of the call for <see cref="ExpressionContext.Row"/>: the value the
    /// call aggregates for that row. A call without an argument, such as <c>COUNT()</c>, counts every row.
    /// </summary>
    public Func<ExpressionContext, object> Argument { get; }
}
