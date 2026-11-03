namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// One token of a formula.
/// </summary>
/// <param name="Kind">The token kind.</param>
/// <param name="Text">The token text: the operator, the identifier, the field key, or the unquoted string.</param>
/// <param name="Position">The zero-based position of the token in the formula.</param>
public sealed record ExpressionToken(ExpressionTokenKind Kind, string Text, int Position);

/// <summary>
/// Identifies the kind of a formula token.
/// </summary>
public enum ExpressionTokenKind
{
    /// <summary>
    /// A number literal.
    /// </summary>
    Number,

    /// <summary>
    /// A quoted text literal.
    /// </summary>
    String,

    /// <summary>
    /// A function name or keyword.
    /// </summary>
    Identifier,

    /// <summary>
    /// A field reference written in square brackets.
    /// </summary>
    Field,

    /// <summary>
    /// An operator or punctuation.
    /// </summary>
    Operator,

    /// <summary>
    /// The end of the formula.
    /// </summary>
    End,
}
