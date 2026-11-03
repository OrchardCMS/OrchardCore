namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// A node of a parsed formula.
/// </summary>
/// <param name="Position">The zero-based position of the node in the formula.</param>
public abstract record ExpressionNode(int Position);

/// <summary>
/// A literal value: a number (<see cref="long"/> or <see cref="decimal"/>), a text, a boolean, or
/// <see langword="null"/>.
/// </summary>
/// <param name="Value">The literal value.</param>
/// <param name="Position">The zero-based position of the node in the formula.</param>
public sealed record LiteralNode(object Value, int Position)
    : ExpressionNode(Position);

/// <summary>
/// A reference to a field, written <c>[alias.Field]</c> or <c>[CalculatedField]</c>.
/// </summary>
/// <param name="Key">The field key.</param>
/// <param name="Position">The zero-based position of the node in the formula.</param>
public sealed record FieldNode(string Key, int Position)
    : ExpressionNode(Position);

/// <summary>
/// A prefix operator applied to one operand.
/// </summary>
/// <param name="Operator">The operator: <c>-</c>, <c>+</c>, or <c>NOT</c>.</param>
/// <param name="Operand">The operand.</param>
/// <param name="Position">The zero-based position of the node in the formula.</param>
public sealed record UnaryNode(string Operator, ExpressionNode Operand, int Position)
    : ExpressionNode(Position);

/// <summary>
/// An operator applied to two operands.
/// </summary>
/// <param name="Operator">The normalized operator: <c>+ - * / % &amp; = != &lt; &lt;= &gt; &gt;= AND OR</c>.</param>
/// <param name="Left">The left operand.</param>
/// <param name="Right">The right operand.</param>
/// <param name="Position">The zero-based position of the node in the formula.</param>
public sealed record BinaryNode(string Operator, ExpressionNode Left, ExpressionNode Right, int Position)
    : ExpressionNode(Position);

/// <summary>
/// A function call.
/// </summary>
/// <param name="Name">The upper-case function name.</param>
/// <param name="Arguments">The arguments.</param>
/// <param name="Position">The zero-based position of the node in the formula.</param>
public sealed record FunctionNode(string Name, IReadOnlyList<ExpressionNode> Arguments, int Position)
    : ExpressionNode(Position);
