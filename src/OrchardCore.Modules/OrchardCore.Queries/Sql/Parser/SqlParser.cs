using System;
using OrchardCore.Queries.Sql.Parser.Expressions;
using OrchardCore.Queries.Sql.Parser.Expressions.Terminals;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace OrchardCore.Queries.Sql.Parser
{
    /*
        Grammar:
        
        Expression -> Experssion + Factor | Expression - Factor | Factor
        Factor -> Factor * Terminal | Factor / Terminal | Terminal
        Terminal -> Identifier | Number | Boolean | String | (Expression)
        Identifier = (Letter)(Letter | Digit)*
        Number -> (Sign)?(Digit)+
        Boolean -> True | False
        String -> "(Letter | Digit)*" | '(Letter | Digit)*'
        Letter -> [a – z] | [A – Z]
        Digit -> [0 - 9]
        Sign -> [+ | -]
    */
    public class SqlParser
    {
        public static readonly IParser<Expression> Expression;

        static SqlParser()
        {
            // Terminals
            var plus = Terms.Char('+');
            var minus = Terms.Char('-');
            var times = Terms.Char('*');
            var divided = Terms.Char('/');
            var openParen = Terms.Char('(');
            var closeParen = Terms.Char(')');

            var number = Terms.Decimal()
                .Then<Expression>(e => new NumberExpression(e));
            var boolean = Terms.Text("TRUE", caseInsensitive: true).Or(Terms.Text("FALSE", caseInsensitive: true))
                .Then<Expression>(e => new BooleanExpression(Convert.ToBoolean(e)));
            var stringLiteral = Terms.String(StringLiteralQuotes.SingleOrDouble)
                .Then<Expression>(e => new StringExpression(e.Text));
            var identifier = Terms.Identifier()
                .Then<Expression>(e => new IdentifierExpression(e.Text));

            // Expressions
            var expression = Deferred<Expression>();
            var groupExpression = Between(openParen, expression, closeParen);
            var terminal = identifier.Or(number).Or(boolean).Or(stringLiteral).Or(groupExpression);
            var unaryExpression = Recursive<Expression>(e => minus
                .And(e)
                .Then<Expression>(e => new NegateExpression(e.Item2))
                .Or(terminal));
            var factor = unaryExpression.And(Star(times.Or(divided).And(unaryExpression)))
                .Then(e =>
                {
                    var result = e.Item1;
                    foreach (var operation in e.Item2)
                    {
                        result = operation.Item1 switch
                        {
                            '*' => new MultiplicationExpression(result, operation.Item2),
                            '/' => new DivisionExpression(result, operation.Item2),
                            _ => null
                        };
                    }

                    return result;
                });

            expression.Parser = factor.And(Star(plus.Or(minus).And(factor)))
                .Then(e =>
                {
                    var result = e.Item1;
                    foreach (var operation in e.Item2)
                    {
                        result = operation.Item1 switch
                        {
                            '+' => new AdditionExpression(result, operation.Item2),
                            '-' => new SubtractionExpression(result, operation.Item2),
                            _ => null
                        };
                    }

                    return result;
                });

            Expression = expression;
        }
    }
}
