
namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// A formula compiled against a scope: its result type, whether it aggregates, the fields it reads, and the
/// delegate that evaluates it.
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
    {
        DataType = dataType;
        IsAggregate = isAggregate;
        References = references;
        Evaluate = evaluate;
    }

    /// <summary>
    /// Gets the type of the values the formula returns.
    /// </summary>
    public DataFieldType DataType { get; }

    /// <summary>
    /// Gets a value indicating whether the formula aggregates a group of rows, and so must be evaluated with
    /// <see cref="ExpressionContext.Group"/> set.
    /// </summary>
    public bool IsAggregate { get; }

    /// <summary>
    /// Gets the keys of the fields the formula reads, including those read through other calculated fields.
    /// </summary>
    public IReadOnlySet<string> References { get; }

    /// <summary>
    /// Gets the delegate that evaluates the formula.
    /// </summary>
    public Func<ExpressionContext, object> Evaluate { get; }
}
