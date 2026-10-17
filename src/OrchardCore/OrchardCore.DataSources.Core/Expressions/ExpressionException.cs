namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// Thrown when a formula cannot be read or does not make sense, such as a missing parenthesis, an unknown
/// field, or an operator applied to the wrong type of value.
/// </summary>
public sealed class ExpressionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionException"/> class.
    /// </summary>
    public ExpressionException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the problem.</param>
    public ExpressionException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the problem.</param>
    /// <param name="innerException">The exception that caused the problem.</param>
    public ExpressionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the problem.</param>
    /// <param name="position">The zero-based position in the formula where the problem was found.</param>
    public ExpressionException(string message, int position)
        : base(message)
    {
        Position = position;
    }

    /// <summary>
    /// Gets the zero-based position in the formula where the problem was found, or <c>-1</c> when unknown.
    /// </summary>
    public int Position { get; } = -1;
}
