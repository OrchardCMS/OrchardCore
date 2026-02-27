namespace OrchardCore.Infrastructure;

/// <summary>
/// Represents the result of an operation.
/// </summary>
/// <typeparam name="TValue">The type of the value returned by the operation.</typeparam>
public class Result<TValue> : Result
{
    /// <summary>
    /// Gets or sets the value returned by the operation.
    /// </summary>
    public TValue Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value returned by the operation.</param>
    /// <param name="succeeded">Indicates whether the operation succeeded.</param>
    /// <param name="errors">The errors that occurred during the operation.</param>
    protected internal Result(TValue value, bool succeeded, List<ResultError> errors)
        : base(succeeded, errors) => Value = value;
}
