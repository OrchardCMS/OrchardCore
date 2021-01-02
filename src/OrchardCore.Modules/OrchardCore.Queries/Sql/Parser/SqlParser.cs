using System;
using OrchardCore.Queries.Sql.Parser.Expressions;
using OrchardCore.Queries.Sql.Parser.Expressions.Terminals;
using Parlot;

namespace OrchardCore.Queries.Sql.Parser
{
    /*
        Grammar:
        
        Expression -> Experssion + Factor | Expression - Factor | Factor
        Factor -> Factor * Terminal | Factor / Terminal | Terminal
        Terminal -> Identifier | Number | Boolean | String
        Identifier = (Letter)(Letter | Digit)*
        Number -> (Sign)?(Digit)+
        Boolean -> True | False
        String -> "(Letter | Digit)*" | '(Letter | Digit)*'
        Letter -> [a – z] | [A – Z]
        Digit -> [0 - 9]
        Sign -> [+ | -]
    */
    public class SqlParser : Parser<Expression>
    {
        private Scanner _scanner;

        public override Expression Parse(string text)
        {
            _scanner = new Scanner(text);

            return ParseExpression();
        }

        private Expression ParseExpression()
        {
            var expression = ParseFactor();

            while (true)
            {
                _scanner.SkipWhiteSpace();

                if (_scanner.ReadChar('+'))
                {
                    _scanner.SkipWhiteSpace();

                    expression = new AdditionExpression(expression, ParseFactor());
                }
                else if (_scanner.ReadChar('-'))
                {
                    _scanner.SkipWhiteSpace();

                    expression = new SubtractionExpression(expression, ParseFactor());
                }
                else
                {
                    break;
                }
            }

            return expression;
        }

        private Expression ParseFactor()
        {
            var expression = ParseUnaryExpression();
            while (true)
            {
                _scanner.SkipWhiteSpace();

                if (_scanner.ReadChar('*'))
                {
                    _scanner.SkipWhiteSpace();

                    expression = new MultiplicationExpression(expression, ParseUnaryExpression());
                }
                else if (_scanner.ReadChar('/'))
                {
                    _scanner.SkipWhiteSpace();

                    expression = new DivisionExpression(expression, ParseUnaryExpression());
                }
                else
                {
                    break;
                }
            }

            return expression;
        }

        private Expression ParseUnaryExpression()
        {
            _scanner.SkipWhiteSpace();

            if (_scanner.ReadChar('-'))
            {
                var expression = ParseUnaryExpression();
                if (expression == null)
                {
                    throw new ParseException("Expected expression after '-'", _scanner.Cursor.Position);
                }

                return new NegateExpression(expression);
            }

            return ParseTerminal();
        }

        private Expression ParseTerminal()
        {
            _scanner.SkipWhiteSpace();

            var token = new TokenResult();
            if (_scanner.ReadIdentifier(token))
            {
                return new IdentifierExpression(token.Span.ToString());
            }
            else if (_scanner.ReadDecimal(token))
            {
                return new NumberExpression(Decimal.Parse(token.Span));
            }
            else if (_scanner.ReadText("TRUE", StringComparer.OrdinalIgnoreCase) || _scanner.ReadText("FALSE", StringComparer.OrdinalIgnoreCase))
            {
                return new BooleanExpression(Boolean.Parse(token.Span.ToString()));
            }
            else if (_scanner.ReadSingleQuotedString(token) || _scanner.ReadDoubleQuotedString(token))
            {
                var stringValue = token.Span.ToString();
                if (stringValue.StartsWith("\""))
                {
                    return new StringExpression(stringValue.Trim('"'));
                }
                else
                {
                    return new StringExpression(stringValue.Trim('\''), StringQuote.Single);
                }
            }
            else
            {
                throw new ParseException("Expected terminal.", _scanner.Cursor.Position);
            }
        }
    }
}
