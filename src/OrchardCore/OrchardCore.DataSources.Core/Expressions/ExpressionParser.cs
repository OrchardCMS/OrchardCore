using System.Globalization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// Parses a formula into a tree. From the lowest precedence to the highest, the grammar is: <c>OR ||</c>,
/// <c>AND &amp;&amp;</c>, <c>NOT !</c>, comparisons (<c>= == != &lt;&gt; &lt; &lt;= &gt; &gt;=</c>), text joining
/// (<c>&amp;</c>), <c>+ -</c>, <c>* / %</c>, unary <c>- +</c>, then literals, fields, function calls, and parentheses.
/// Keywords and function names are case-insensitive.
/// </summary>
public sealed class ExpressionParser
{
    private const int MaximumDepth = 64;

    private readonly IReadOnlyList<ExpressionToken> _tokens;
    private readonly IStringLocalizer S;

    private int _index;
    private int _depth;

    private ExpressionParser(IReadOnlyList<ExpressionToken> tokens, IStringLocalizer localizer)
    {
        _tokens = tokens;
        S = localizer;
    }

    /// <summary>
    /// Parses a formula.
    /// </summary>
    /// <param name="formula">The formula.</param>
    /// <param name="localizer">The localizer used for error messages.</param>
    /// <returns>The root node.</returns>
    /// <exception cref="ExpressionException">The formula is empty or malformed.</exception>
    public static ExpressionNode Parse(string formula, IStringLocalizer localizer)
    {
        ArgumentNullException.ThrowIfNull(localizer);

        if (string.IsNullOrWhiteSpace(formula))
        {
            throw new ExpressionException(localizer["The formula is empty."], 0);
        }

        var parser = new ExpressionParser(ExpressionLexer.Tokenize(formula, localizer), localizer);
        var root = parser.ParseOr();
        var next = parser.Peek();

        if (next.Kind != ExpressionTokenKind.End)
        {
            throw new ExpressionException(localizer["Unexpected '{0}' at position {1}.", next.Text, next.Position + 1], next.Position);
        }

        return root;
    }

    private ExpressionNode ParseOr()
    {
        var left = ParseAnd();

        while (MatchKeyword("OR") || MatchOperator("||"))
        {
            var position = Previous().Position;
            left = new BinaryNode("OR", left, ParseAnd(), position);
        }

        return left;
    }

    private ExpressionNode ParseAnd()
    {
        var left = ParseNot();

        while (MatchKeyword("AND") || MatchOperator("&&"))
        {
            var position = Previous().Position;
            left = new BinaryNode("AND", left, ParseNot(), position);
        }

        return left;
    }

    private ExpressionNode ParseNot()
    {
        if (MatchKeyword("NOT") || MatchOperator("!"))
        {
            var position = Previous().Position;

            return Nested(() => new UnaryNode("NOT", ParseNot(), position));
        }

        return ParseComparison();
    }

    private ExpressionNode ParseComparison()
    {
        var left = ParseConcatenation();

        while (true)
        {
            var token = Peek();

            if (token.Kind != ExpressionTokenKind.Operator)
            {
                return left;
            }

            var normalized = token.Text switch
            {
                "=" or "==" => "=",
                "!=" or "<>" => "!=",
                "<" or "<=" or ">" or ">=" => token.Text,
                _ => null,
            };

            if (normalized is null)
            {
                return left;
            }

            _index++;
            left = new BinaryNode(normalized, left, ParseConcatenation(), token.Position);
        }
    }

    private ExpressionNode ParseConcatenation()
    {
        var left = ParseAdditive();

        while (MatchOperator("&"))
        {
            var position = Previous().Position;
            left = new BinaryNode("&", left, ParseAdditive(), position);
        }

        return left;
    }

    private ExpressionNode ParseAdditive()
    {
        var left = ParseMultiplicative();

        while (MatchOperator("+") || MatchOperator("-"))
        {
            var token = Previous();
            left = new BinaryNode(token.Text, left, ParseMultiplicative(), token.Position);
        }

        return left;
    }

    private ExpressionNode ParseMultiplicative()
    {
        var left = ParseUnary();

        while (MatchOperator("*") || MatchOperator("/") || MatchOperator("%"))
        {
            var token = Previous();
            left = new BinaryNode(token.Text, left, ParseUnary(), token.Position);
        }

        return left;
    }

    private ExpressionNode ParseUnary()
    {
        if (MatchOperator("-") || MatchOperator("+"))
        {
            var token = Previous();

            return Nested(() => new UnaryNode(token.Text, ParseUnary(), token.Position));
        }

        return ParsePrimary();
    }

    private ExpressionNode ParsePrimary()
    {
        var token = Advance();

        switch (token.Kind)
        {
            case ExpressionTokenKind.Number:
                return new LiteralNode(ParseNumber(token), token.Position);

            case ExpressionTokenKind.String:
                return new LiteralNode(token.Text, token.Position);

            case ExpressionTokenKind.Field:
                if (string.IsNullOrWhiteSpace(token.Text))
                {
                    throw new ExpressionException(S["The field reference at position {0} is empty.", token.Position + 1], token.Position);
                }

                return new FieldNode(token.Text.Trim(), token.Position);

            case ExpressionTokenKind.Identifier:
                return ParseIdentifier(token);

            case ExpressionTokenKind.Operator when token.Text == "(":
                var inner = Nested(ParseOr);
                Expect(")");

                return inner;

            case ExpressionTokenKind.End:
                throw new ExpressionException(S["The formula ends unexpectedly."], token.Position);

            default:
                throw new ExpressionException(S["Unexpected '{0}' at position {1}.", token.Text, token.Position + 1], token.Position);
        }
    }

    private ExpressionNode ParseIdentifier(ExpressionToken token)
    {
        var name = token.Text.ToUpperInvariant();

        switch (name)
        {
            case "TRUE":
                return new LiteralNode(true, token.Position);

            case "FALSE":
                return new LiteralNode(false, token.Position);

            case "NULL":
                return new LiteralNode(null, token.Position);
        }

        if (!MatchOperator("("))
        {
            throw new ExpressionException(S["Unknown name '{0}' at position {1}. Write fields in square brackets, such as [Name].", token.Text, token.Position + 1], token.Position);
        }

        var arguments = new List<ExpressionNode>();

        if (!MatchOperator(")"))
        {
            do
            {
                arguments.Add(Nested(ParseOr));
            }
            while (MatchOperator(","));

            Expect(")");
        }

        return new FunctionNode(name, arguments, token.Position);
    }

    private object ParseNumber(ExpressionToken token)
    {
        if (!token.Text.Contains('.', StringComparison.Ordinal) &&
            long.TryParse(token.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var whole))
        {
            return whole;
        }

        if (decimal.TryParse(token.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
        {
            return number;
        }

        throw new ExpressionException(S["The number '{0}' at position {1} is too large.", token.Text, token.Position + 1], token.Position);
    }

    private T Nested<T>(Func<T> parse)
    {
        if (++_depth > MaximumDepth)
        {
            throw new ExpressionException(S["The formula is nested too deeply."], Peek().Position);
        }

        try
        {
            return parse();
        }
        finally
        {
            _depth--;
        }
    }

    private void Expect(string text)
    {
        if (!MatchOperator(text))
        {
            var token = Peek();

            throw token.Kind == ExpressionTokenKind.End
                ? new ExpressionException(S["Expected '{0}' at the end of the formula.", text], token.Position)
                : new ExpressionException(S["Expected '{0}' at position {1} but found '{2}'.", text, token.Position + 1, token.Text], token.Position);
        }
    }

    private bool MatchOperator(string text)
    {
        var token = Peek();

        if (token.Kind == ExpressionTokenKind.Operator && token.Text == text)
        {
            _index++;

            return true;
        }

        return false;
    }

    private bool MatchKeyword(string keyword)
    {
        var token = Peek();

        if (token.Kind == ExpressionTokenKind.Identifier &&
            string.Equals(token.Text, keyword, StringComparison.OrdinalIgnoreCase) &&
            !IsFunctionCall())
        {
            _index++;

            return true;
        }

        return false;
    }

    private bool IsFunctionCall()
    {
        return _index + 1 < _tokens.Count &&
            _tokens[_index + 1].Kind == ExpressionTokenKind.Operator &&
            _tokens[_index + 1].Text == "(";
    }

    private ExpressionToken Peek()
    {
        return _tokens[Math.Min(_index, _tokens.Count - 1)];
    }

    private ExpressionToken Advance()
    {
        var token = Peek();

        if (token.Kind != ExpressionTokenKind.End)
        {
            _index++;
        }

        return token;
    }

    private ExpressionToken Previous()
    {
        return _tokens[_index - 1];
    }
}
