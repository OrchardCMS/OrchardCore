using System.Globalization;

namespace OrchardCore.Localization.PortableObject;

/// <summary>
/// Parses the value of the <c>Plural-Forms</c> header of a portable object (PO) file,
/// e.g. <c>nplurals=3; plural=(n==1) ? 0 : (n>=2 &amp;&amp; n&lt;=4) ? 1 : 2;</c>, into a <see cref="PluralizationRuleDelegate"/>.
/// </summary>
/// <remarks>
/// The plural expression supports the gettext grammar: the <c>n</c> variable, non-negative integer literals,
/// the <c>?:</c>, <c>||</c>, <c>&amp;&amp;</c>, <c>!</c>, <c>==</c>, <c>!=</c>, <c>&lt;</c>, <c>&gt;</c>, <c>&lt;=</c>, <c>&gt;=</c>,
/// <c>+</c>, <c>-</c>, <c>*</c>, <c>/</c> and <c>%</c> operators, and parentheses, with the C precedence and associativity.
/// The expression is interpreted, never compiled. A division or a modulo by zero evaluates to <c>0</c>, and a plural form
/// outside of the <c>0</c> to <c>nplurals - 1</c> range selects the first form, like gettext does.
/// </remarks>
public static class PluralFormsParser
{
    private const int MaxPluralCount = 100;
    private const int MaxExpressionLength = 1024;
    private const int MaxDepth = 64;

    /// <summary>
    /// Parses the value of a <c>Plural-Forms</c> header.
    /// </summary>
    /// <param name="pluralForms">The header value, e.g. <c>nplurals=2; plural=n != 1;</c>.</param>
    /// <returns>The pluralization rule, or <c>null</c> if the value is missing, invalid or unsupported.</returns>
    public static PluralizationRuleDelegate Parse(string pluralForms)
        => TryParse(pluralForms, out _, out var rule) ? rule : null;

    /// <summary>
    /// Tries to parse the value of a <c>Plural-Forms</c> header.
    /// </summary>
    /// <param name="pluralForms">The header value, e.g. <c>nplurals=2; plural=n != 1;</c>.</param>
    /// <param name="pluralCount">The number of plural forms, defined by <c>nplurals</c>.</param>
    /// <param name="rule">The pluralization rule, defined by <c>plural</c>.</param>
    /// <returns><c>true</c> if the value could be parsed, otherwise <c>false</c>.</returns>
    public static bool TryParse(string pluralForms, out int pluralCount, out PluralizationRuleDelegate rule)
    {
        pluralCount = 0;
        rule = null;

        if (string.IsNullOrWhiteSpace(pluralForms))
        {
            return false;
        }

        string nplurals = null;
        string plural = null;

        foreach (var part in pluralForms.Split(';'))
        {
            var separatorIndex = part.IndexOf('=');
            if (separatorIndex <= 0)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    return false;
                }

                continue;
            }

            var name = part[..separatorIndex].Trim();
            var value = part[(separatorIndex + 1)..].Trim();

            if (string.Equals(name, "nplurals", StringComparison.OrdinalIgnoreCase))
            {
                nplurals = value;
            }
            else if (string.Equals(name, "plural", StringComparison.OrdinalIgnoreCase))
            {
                plural = value;
            }
            else
            {
                return false;
            }
        }

        if (nplurals == null ||
            plural == null ||
            !int.TryParse(nplurals, NumberStyles.None, CultureInfo.InvariantCulture, out var count) ||
            count < 1 ||
            count > MaxPluralCount)
        {
            return false;
        }

        var expression = ExpressionParser.Parse(plural);
        if (expression == null)
        {
            return false;
        }

        pluralCount = count;
        rule = n =>
        {
            var form = expression(Math.Abs((long)n));

            return form >= 0 && form < count ? (int)form : 0;
        };

        return true;
    }

    private sealed class ExpressionParser
    {
        private readonly string _text;
        private int _position;
        private int _depth;

        private ExpressionParser(string text)
        {
            _text = text;
        }

        public static Func<long, long> Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length > MaxExpressionLength)
            {
                return null;
            }

            var parser = new ExpressionParser(text);
            var expression = parser.ParseTernary();
            parser.SkipWhitespace();

            return expression != null && parser._position == text.Length ? expression : null;
        }

        private Func<long, long> ParseTernary()
        {
            if (++_depth > MaxDepth)
            {
                return null;
            }

            var condition = ParseOr();
            if (condition != null && TryConsume("?"))
            {
                var whenTrue = ParseTernary();
                if (whenTrue == null || !TryConsume(":"))
                {
                    return null;
                }

                var whenFalse = ParseTernary();
                if (whenFalse == null)
                {
                    return null;
                }

                var test = condition;
                condition = n => test(n) != 0 ? whenTrue(n) : whenFalse(n);
            }

            _depth--;

            return condition;
        }

        private Func<long, long> ParseOr()
        {
            var left = ParseAnd();
            while (left != null && TryConsume("||"))
            {
                var right = ParseAnd();
                if (right == null)
                {
                    return null;
                }

                var l = left;
                left = n => l(n) != 0 || right(n) != 0 ? 1 : 0;
            }

            return left;
        }

        private Func<long, long> ParseAnd()
        {
            var left = ParseEquality();
            while (left != null && TryConsume("&&"))
            {
                var right = ParseEquality();
                if (right == null)
                {
                    return null;
                }

                var l = left;
                left = n => l(n) != 0 && right(n) != 0 ? 1 : 0;
            }

            return left;
        }

        private Func<long, long> ParseEquality()
        {
            var left = ParseRelational();
            while (left != null)
            {
                var l = left;
                Func<long, long> right;

                if (TryConsume("=="))
                {
                    right = ParseRelational();
                    left = right == null ? null : n => l(n) == right(n) ? 1 : 0;
                }
                else if (TryConsume("!="))
                {
                    right = ParseRelational();
                    left = right == null ? null : n => l(n) != right(n) ? 1 : 0;
                }
                else
                {
                    break;
                }
            }

            return left;
        }

        private Func<long, long> ParseRelational()
        {
            var left = ParseAdditive();
            while (left != null)
            {
                var l = left;
                Func<long, long> right;

                if (TryConsume("<="))
                {
                    right = ParseAdditive();
                    left = right == null ? null : n => l(n) <= right(n) ? 1 : 0;
                }
                else if (TryConsume(">="))
                {
                    right = ParseAdditive();
                    left = right == null ? null : n => l(n) >= right(n) ? 1 : 0;
                }
                else if (TryConsume("<"))
                {
                    right = ParseAdditive();
                    left = right == null ? null : n => l(n) < right(n) ? 1 : 0;
                }
                else if (TryConsume(">"))
                {
                    right = ParseAdditive();
                    left = right == null ? null : n => l(n) > right(n) ? 1 : 0;
                }
                else
                {
                    break;
                }
            }

            return left;
        }

        private Func<long, long> ParseAdditive()
        {
            var left = ParseMultiplicative();
            while (left != null)
            {
                var l = left;
                Func<long, long> right;

                if (TryConsume("+"))
                {
                    right = ParseMultiplicative();
                    left = right == null ? null : n => unchecked(l(n) + right(n));
                }
                else if (TryConsume("-"))
                {
                    right = ParseMultiplicative();
                    left = right == null ? null : n => unchecked(l(n) - right(n));
                }
                else
                {
                    break;
                }
            }

            return left;
        }

        private Func<long, long> ParseMultiplicative()
        {
            var left = ParseUnary();
            while (left != null)
            {
                var l = left;
                Func<long, long> right;

                if (TryConsume("*"))
                {
                    right = ParseUnary();
                    left = right == null ? null : n => unchecked(l(n) * right(n));
                }
                else if (TryConsume("/"))
                {
                    right = ParseUnary();
                    left = right == null ? null : n => Divide(l(n), right(n));
                }
                else if (TryConsume("%"))
                {
                    right = ParseUnary();
                    left = right == null ? null : n => Modulo(l(n), right(n));
                }
                else
                {
                    break;
                }
            }

            return left;
        }

        private Func<long, long> ParseUnary()
        {
            if (TryConsume("!"))
            {
                if (++_depth > MaxDepth)
                {
                    return null;
                }

                var operand = ParseUnary();
                _depth--;

                return operand == null ? null : n => operand(n) == 0 ? 1 : 0;
            }

            return ParsePrimary();
        }

        private Func<long, long> ParsePrimary()
        {
            SkipWhitespace();

            if (_position >= _text.Length)
            {
                return null;
            }

            var c = _text[_position];

            if (c == 'n')
            {
                _position++;

                return n => n;
            }

            if (char.IsAsciiDigit(c))
            {
                var start = _position;
                while (_position < _text.Length && char.IsAsciiDigit(_text[_position]))
                {
                    _position++;
                }

                if (!long.TryParse(_text.AsSpan(start, _position - start), NumberStyles.None, CultureInfo.InvariantCulture, out var value))
                {
                    return null;
                }

                return _ => value;
            }

            if (TryConsume("("))
            {
                var expression = ParseTernary();

                return expression != null && TryConsume(")") ? expression : null;
            }

            return null;
        }

        private bool TryConsume(string token)
        {
            SkipWhitespace();

            if (string.CompareOrdinal(_text, _position, token, 0, token.Length) != 0)
            {
                return false;
            }

            // Don't read the first character of a two-character operator as a single-character one, e.g. '!' of '!=', or '<' of '<='.
            if (token.Length == 1 && _position + 1 < _text.Length)
            {
                var next = _text[_position + 1];
                if ((token == "!" && next == '=') ||
                    ((token == "<" || token == ">") && next == '='))
                {
                    return false;
                }
            }

            _position += token.Length;

            return true;
        }

        private void SkipWhitespace()
        {
            while (_position < _text.Length && char.IsWhiteSpace(_text[_position]))
            {
                _position++;
            }
        }

        private static long Divide(long left, long right)
            => right == 0 || (left == long.MinValue && right == -1) ? 0 : left / right;

        private static long Modulo(long left, long right)
            => right == 0 || right == -1 ? 0 : left % right;
    }
}
