using System.Text;
using Microsoft.Extensions.Localization;

namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// Splits a formula into tokens. Text is quoted with single or double quotes (a doubled quote escapes itself),
/// fields are written in square brackets (a doubled closing bracket escapes itself), and numbers use a dot as the
/// decimal separator.
/// </summary>
public static class ExpressionLexer
{
    private static readonly string[] _operators =
    [
        "==",
        "!=",
        "<>",
        "<=",
        ">=",
        "&&",
        "||",
        "+",
        "-",
        "*",
        "/",
        "%",
        "(",
        ")",
        ",",
        "=",
        "<",
        ">",
        "!",
        "&",
    ];

    /// <summary>
    /// Splits a formula into tokens, ending with an <see cref="ExpressionTokenKind.End"/> token.
    /// </summary>
    /// <param name="formula">The formula.</param>
    /// <param name="localizer">The localizer used for error messages.</param>
    /// <returns>The tokens.</returns>
    public static IReadOnlyList<ExpressionToken> Tokenize(string formula, IStringLocalizer localizer)
    {
        ArgumentNullException.ThrowIfNull(localizer);

        var tokens = new List<ExpressionToken>();
        var text = formula ?? string.Empty;
        var index = 0;

        while (index < text.Length)
        {
            var current = text[index];

            if (char.IsWhiteSpace(current))
            {
                index++;

                continue;
            }

            if (char.IsDigit(current) || (current == '.' && index + 1 < text.Length && char.IsDigit(text[index + 1])))
            {
                tokens.Add(ReadNumber(text, ref index));

                continue;
            }

            if (current is '\'' or '"')
            {
                tokens.Add(ReadQuoted(text, ref index, current, ExpressionTokenKind.String, localizer));

                continue;
            }

            if (current == '[')
            {
                tokens.Add(ReadQuoted(text, ref index, ']', ExpressionTokenKind.Field, localizer));

                continue;
            }

            if (char.IsLetter(current) || current == '_')
            {
                var start = index;

                while (index < text.Length && (char.IsLetterOrDigit(text[index]) || text[index] == '_'))
                {
                    index++;
                }

                tokens.Add(new ExpressionToken(ExpressionTokenKind.Identifier, text[start..index], start));

                continue;
            }

            var matched = _operators.FirstOrDefault(op => string.CompareOrdinal(text, index, op, 0, op.Length) == 0);

            if (matched is null)
            {
                throw new ExpressionException(localizer["Unexpected character '{0}' at position {1}.", current, index + 1], index);
            }

            tokens.Add(new ExpressionToken(ExpressionTokenKind.Operator, matched, index));
            index += matched.Length;
        }

        tokens.Add(new ExpressionToken(ExpressionTokenKind.End, string.Empty, text.Length));

        return tokens;
    }

    private static ExpressionToken ReadNumber(string text, ref int index)
    {
        var start = index;
        var seenDot = false;

        while (index < text.Length && (char.IsDigit(text[index]) || (text[index] == '.' && !seenDot)))
        {
            if (text[index] == '.')
            {
                seenDot = true;
            }

            index++;
        }

        return new ExpressionToken(ExpressionTokenKind.Number, text[start..index], start);
    }

    private static ExpressionToken ReadQuoted(string text, ref int index, char closing, ExpressionTokenKind kind, IStringLocalizer localizer)
    {
        var start = index;
        var builder = new StringBuilder();

        index++;

        while (index < text.Length)
        {
            var current = text[index];

            if (current == closing)
            {
                if (index + 1 < text.Length && text[index + 1] == closing)
                {
                    builder.Append(closing);
                    index += 2;

                    continue;
                }

                index++;

                return new ExpressionToken(kind, builder.ToString(), start);
            }

            builder.Append(current);
            index++;
        }

        throw kind == ExpressionTokenKind.Field
            ? new ExpressionException(localizer["The field reference that starts at position {0} has no closing ']'.", start + 1], start)
            : new ExpressionException(localizer["The text that starts at position {0} has no closing quote.", start + 1], start);
    }
}
