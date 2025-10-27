#nullable enable

using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace OrchardCore.Queries.Sql;

/// <summary>
/// SQL parser using Parlot library (compatible with Parlot 1.5.1)
/// </summary>
public class ParlotSqlParser
{
    public static readonly Parser<StatementList> Statements;

    static ParlotSqlParser()
    {
        // Basic terminals
        var COMMA = Terms.Char(',');
        var DOT = Terms.Char('.');
        var SEMICOLON = Terms.Char(';');
        var LPAREN = Terms.Char('(');
        var RPAREN = Terms.Char(')');
        var AT = Terms.Char('@');
        var STAR = Terms.Char('*');
        var EQ = Terms.Char('=');
        var COLON = Terms.Char(':');

        // Keywords
        var SELECT = Terms.Text("SELECT", caseInsensitive: true);
        var FROM = Terms.Text("FROM", caseInsensitive: true);
        var WHERE = Terms.Text("WHERE", caseInsensitive: true);
        var AS = Terms.Text("AS", caseInsensitive: true);
        var JOIN = Terms.Text("JOIN", caseInsensitive: true);
        var INNER = Terms.Text("INNER", caseInsensitive: true);
        var LEFT = Terms.Text("LEFT", caseInsensitive: true);
        var RIGHT = Terms.Text("RIGHT", caseInsensitive: true);
        var ON = Terms.Text("ON", caseInsensitive: true);
        var GROUP = Terms.Text("GROUP", caseInsensitive: true);
        var BY = Terms.Text("BY", caseInsensitive: true);
        var HAVING = Terms.Text("HAVING", caseInsensitive: true);
        var ORDER = Terms.Text("ORDER", caseInsensitive: true);
        var ASC = Terms.Text("ASC", caseInsensitive: true);
        var DESC = Terms.Text("DESC", caseInsensitive: true);
        var LIMIT = Terms.Text("LIMIT", caseInsensitive: true);
        var OFFSET = Terms.Text("OFFSET", caseInsensitive: true);
        var UNION = Terms.Text("UNION", caseInsensitive: true);
        var ALL = Terms.Text("ALL", caseInsensitive: true);
        var DISTINCT = Terms.Text("DISTINCT", caseInsensitive: true);
        var WITH = Terms.Text("WITH", caseInsensitive: true);
        var AND = Terms.Text("AND", caseInsensitive: true);
        var OR = Terms.Text("OR", caseInsensitive: true);
        var NOT = Terms.Text("NOT", caseInsensitive: true);
        var BETWEEN = Terms.Text("BETWEEN", caseInsensitive: true);
        var IN = Terms.Text("IN", caseInsensitive: true);
        var LIKE = Terms.Text("LIKE", caseInsensitive: true);
        var TRUE = Terms.Text("TRUE", caseInsensitive: true);
        var FALSE = Terms.Text("FALSE", caseInsensitive: true);
        var OVER = Terms.Text("OVER", caseInsensitive: true);
        var PARTITION = Terms.Text("PARTITION", caseInsensitive: true);

        // Literals
        var numberLiteral = Terms.Decimal().Then<Expression>(d => new LiteralExpression<decimal>(d));

        var stringLiteral = Terms.String(StringLiteralQuotes.Single)
            .Then<Expression>(s => new LiteralExpression<string>(s.ToString()));

        var booleanLiteral = TRUE.Then<Expression>(new LiteralExpression<bool>(true))
            .Or(FALSE.Then<Expression>(new LiteralExpression<bool>(false)));

        // Identifiers - simplified version compatible with Parlot 1.5.1
        // Support: identifier, [identifier], "identifier"
        var bracketedIdentifier = Between(Terms.Char('['), Terms.Identifier(), Terms.Char(']'));
        var quotedIdentifier = Between(Terms.Char('"'), Terms.Identifier(), Terms.Char('"'));
        var simpleIdentifier = Terms.Identifier().Or(bracketedIdentifier).Or(quotedIdentifier);

        var identifier = Separated(DOT, simpleIdentifier)
            .Then(parts => new Identifier(parts.Select(p => p.ToString()).ToArray()));

        // Deferred parsers
        var expression = Deferred<Expression>();
        var selectStatement = Deferred<SelectStatement>();
        var columnItem = Deferred<ColumnItem>();
        var orderByItem = Deferred<OrderByItem>();

        // Expression list
        var expressionList = Separated(COMMA, expression);

        // Function arguments
        var starArg = STAR.Then<FunctionArguments>(_ => new StarArgument());
        var selectArg = selectStatement.Then<FunctionArguments>(s => new SelectStatementArgument(s));
        var exprListArg = expressionList.Then<FunctionArguments>(exprs => new ExpressionListArguments(exprs));
        var emptyArg = Always<FunctionArguments>(new EmptyArguments());
        var functionArgs = starArg.Or(selectArg).Or(exprListArg).Or(emptyArg);

        // Function call
        var functionCall = identifier.And(Between(LPAREN, functionArgs, RPAREN))
            .Then<Expression>(x => new FunctionCall(x.Item1, x.Item2));

        // Parameter with optional default value
        var defaultValue = COLON.SkipAnd(expression);
        var parameter = AT.And(identifier).And(defaultValue.Optional()).Then<Expression>(x =>
        {
            var (_, name, defaultVal) = x;
            return new ParameterExpression(name, defaultVal.OrSome(null));
        });

        // Tuple
        var tuple = Between(LPAREN, expressionList, RPAREN)
            .Then<Expression>(exprs => new TupleExpression(exprs));

        // Parenthesized select
        var parSelectStatement = Between(LPAREN, selectStatement, RPAREN)
            .Then<Expression>(s => new ParenthesizedSelectStatement(s));

        // Basic term
        var identifierExpr = identifier.Then<Expression>(id => new IdentifierExpression(id));

        var term = functionCall
            .Or(parameter)
            .Or(parSelectStatement)
            .Or(tuple)
            .Or(booleanLiteral)
            .Or(stringLiteral)
            .Or(numberLiteral)
            .Or(identifierExpr);

        // Unary expressions
        var unaryMinus = Terms.Char('-').And(term).Then<Expression>(x => new UnaryExpression(UnaryOperator.Minus, x.Item2));
        var unaryPlus = Terms.Char('+').And(term).Then<Expression>(x => new UnaryExpression(UnaryOperator.Plus, x.Item2));
        var unaryNot = NOT.And(term).Then<Expression>(x => new UnaryExpression(UnaryOperator.Not, x.Item2));
        var unaryBitwiseNot = Terms.Char('~').And(term).Then<Expression>(x => new UnaryExpression(UnaryOperator.BitwiseNot, x.Item2));

        var unaryExpr = unaryMinus.Or(unaryPlus).Or(unaryNot).Or(unaryBitwiseNot);
        var primary = unaryExpr.Or(term);

        // Binary operators with proper precedence
        var notLike = NOT.AndSkip(LIKE);
        var likeOp = notLike.Or(LIKE);

        var multiplicative = primary.LeftAssociative(
            (Terms.Char('*'), (a, b) => new BinaryExpression(a, BinaryOperator.Multiply, b)),
            (Terms.Char('/'), (a, b) => new BinaryExpression(a, BinaryOperator.Divide, b)),
            (Terms.Char('%'), (a, b) => new BinaryExpression(a, BinaryOperator.Modulo, b))
        );

        var additive = multiplicative.LeftAssociative(
            (Terms.Char('+'), (a, b) => new BinaryExpression(a, BinaryOperator.Add, b)),
            (Terms.Char('-'), (a, b) => new BinaryExpression(a, BinaryOperator.Subtract, b))
        );

        var comparisonText = additive.LeftAssociative(
            (Terms.Text(">="), (a, b) => new BinaryExpression(a, BinaryOperator.GreaterThanOrEqual, b)),
            (Terms.Text("<="), (a, b) => new BinaryExpression(a, BinaryOperator.LessThanOrEqual, b)),
            (Terms.Text("<>"), (a, b) => new BinaryExpression(a, BinaryOperator.NotEqual, b)),
            (Terms.Text("!="), (a, b) => new BinaryExpression(a, BinaryOperator.NotEqualAlt, b)),
            (Terms.Text("!<"), (a, b) => new BinaryExpression(a, BinaryOperator.NotLessThan, b)),
            (Terms.Text("!>"), (a, b) => new BinaryExpression(a, BinaryOperator.NotGreaterThan, b))
        );

        var comparisonChar = comparisonText.LeftAssociative(
            (Terms.Char('>'), (a, b) => new BinaryExpression(a, BinaryOperator.GreaterThan, b)),
            (Terms.Char('<'), (a, b) => new BinaryExpression(a, BinaryOperator.LessThan, b)),
            (EQ, (a, b) => new BinaryExpression(a, BinaryOperator.Equal, b))
        );

        var comparison = comparisonChar.LeftAssociative(
            (notLike, (a, b) => new BinaryExpression(a, BinaryOperator.NotLike, b)),
            (LIKE, (a, b) => new BinaryExpression(a, BinaryOperator.Like, b))
        );

        var bitwise = comparison.LeftAssociative(
            (Terms.Char('^'), (a, b) => new BinaryExpression(a, BinaryOperator.BitwiseXor, b)),
            (Terms.Char('&'), (a, b) => new BinaryExpression(a, BinaryOperator.BitwiseAnd, b)),
            (Terms.Char('|'), (a, b) => new BinaryExpression(a, BinaryOperator.BitwiseOr, b))
        );

        var andExpr = bitwise.LeftAssociative(
            (AND, (a, b) => new BinaryExpression(a, BinaryOperator.And, b))
        );

        var orExpr = andExpr.LeftAssociative(
            (OR, (a, b) => new BinaryExpression(a, BinaryOperator.Or, b))
        );

        // BETWEEN and IN expressions
        var betweenExpr = andExpr.And(NOT.Optional()).AndSkip(BETWEEN).And(bitwise).AndSkip(AND).And(bitwise)
            .Then<Expression>(result =>
            {
                var (expr, notKeyword, lower, upper) = result;
                return new BetweenExpression(expr, lower, upper, notKeyword.HasValue);
            });

        var inExpr = andExpr.And(NOT.Optional()).AndSkip(IN).AndSkip(LPAREN).And(expressionList).AndSkip(RPAREN)
            .Then<Expression>(result =>
            {
                var (expr, notKeyword, values) = result;
                return new InExpression(expr, values, notKeyword.HasValue);
            });

        expression.Parser = betweenExpr.Or(inExpr).Or(orExpr);

        // Column source
        var columnSourceId = identifier.Then<ColumnSource>(id => new ColumnSourceIdentifier(id));

        // Deferred for OVER clause components
        var columnItemList = Separated(COMMA, columnItem.Or(STAR.Then(new ColumnItem(new ColumnSourceIdentifier(new Identifier("*")), null))));
        var orderByList = Separated(COMMA, orderByItem);

        var orderByClause = ORDER.AndSkip(BY).And(orderByList)
            .Then(x => new OrderByClause(x.Item2));

        var partitionBy = PARTITION.AndSkip(BY).And(columnItemList)
            .Then(x => new PartitionByClause(x.Item2));

        var overClause = OVER.AndSkip(LPAREN).And(partitionBy.Optional()).And(orderByClause.Optional()).AndSkip(RPAREN)
            .Then(result =>
            {
                var (_, partition, orderBy) = result;
                return new OverClause(
                    partition.OrSome(null),
                    orderBy.OrSome(null)
                );
            });

        var columnSourceFunc = functionCall.And(overClause.Optional())
            .Then<ColumnSource>(result =>
            {
                var (func, over) = result;
                return new ColumnSourceFunction((FunctionCall)func, over.OrSome(null));
            });

        var columnSource = columnSourceFunc.Or(columnSourceId);

        // Column item with alias
        var columnAlias = AS.SkipAnd(identifier);

        columnItem.Parser = columnSource.And(columnAlias.Optional())
            .Then(result =>
            {
                var (source, alias) = result;
                return new ColumnItem(source, alias.OrSome(null));
            });

        // Table source
        var tableAlias = AS.SkipAnd(identifier);

        var tableSourceItem = identifier.And(tableAlias.Optional())
            .Then(result =>
            {
                var (id, alias) = result;
                return new TableSourceItem(id, alias.OrSome(null));
            });

        // Deferred union statement list for subqueries
        var unionStatementList = Deferred<IReadOnlyList<UnionStatement>>();

        var tableSourceSubQuery = LPAREN.SkipAnd(unionStatementList).AndSkip(RPAREN).AndSkip(AS).And(simpleIdentifier)
            .Then<TableSource>(result =>
            {
                var (query, alias) = result;
                return new TableSourceSubQuery(query, alias.ToString());
            });

        var tableSourceItemAsTableSource = tableSourceItem.Then<TableSource>(t => t);
        var tableSource = tableSourceSubQuery.Or(tableSourceItemAsTableSource);
        var tableSourceList = Separated(COMMA, tableSource);

        // Join
        var joinKind = INNER.Then(JoinKind.Inner)
            .Or(LEFT.Then(JoinKind.Left))
            .Or(RIGHT.Then(JoinKind.Right));

        var joinCondition = ON.SkipAnd(andExpr);
        var tableSourceItemList = Separated(COMMA, tableSourceItem);

        var joinStatement = joinKind.Else(JoinKind.None).AndSkip(JOIN).And(tableSourceItemList).And(joinCondition)
            .Then(result =>
            {
                var kind = result.Item1;
                var tables = result.Item2;
                var conditions = result.Item3;
                return new JoinStatement(tables, conditions, kind);
            });

        var joins = ZeroOrMany(joinStatement);

        // FROM clause
        var fromClause = FROM.SkipAnd(tableSourceList).And(joins)
            .Then(result =>
            {
                var tables = result.Item1;
                var joinList = result.Item2;
                return new FromClause(tables, joinList.Any() ? joinList : null);
            });

        // WHERE clause
        var whereClause = WHERE.And(expression).Then(x => new WhereClause(x.Item2));

        // GROUP BY clause
        var columnSourceList = Separated(COMMA, columnSource);
        var groupByClause = GROUP.AndSkip(BY).And(columnSourceList)
            .Then(x => new GroupByClause(x.Item2));

        // HAVING clause
        var havingClause = HAVING.And(expression).Then(x => new HavingClause(x.Item2));

        // ORDER BY item
        var orderDirection = ASC.Then(OrderDirection.Asc).Or(DESC.Then(OrderDirection.Desc));

        orderByItem.Parser = identifier.And(orderDirection.Optional())
            .Then(result =>
            {
                var (id, dir) = result;
                return new OrderByItem(id, dir.OrSome(OrderDirection.NotSpecified));
            });

        // LIMIT and OFFSET clauses
        var limitClause = LIMIT.And(expression).Then(x => new LimitClause(x.Item2));
        var offsetClause = OFFSET.And(expression).Then(x => new OffsetClause(x.Item2));

        // SELECT statement
        var selectRestriction = ALL.Then(SelectRestriction.All).Or(DISTINCT.Then(SelectRestriction.Distinct));

        selectStatement.Parser = SELECT
            .SkipAnd(selectRestriction.Else(SelectRestriction.NotSpecified))
            .And(columnItemList)
            .And(fromClause.Optional())
            .And(whereClause.Optional())
            .And(groupByClause.Optional())
            .And(havingClause.Optional())
            .And(orderByClause.Optional())
            .And(limitClause.Optional())
            .And(offsetClause.Optional())
            .Then(result =>
            {
                var ((restriction, columns, from, where, groupBy, having, orderBy), limit, offset) = result;

                return new SelectStatement(
                    columns,
                    restriction,
                    from.OrSome(null),
                    where.OrSome(null),
                    groupBy.OrSome(null),
                    having.OrSome(null),
                    orderBy.OrSome(null),
                    limit.OrSome(null),
                    offset.OrSome(null)
                );
            });

        // WITH clause (CTEs)
        var columnNames = Separated(COMMA, simpleIdentifier)
            .Then(names => names.Select(n => n.ToString()).ToArray());

        var cteColumnList = Between(LPAREN, columnNames, RPAREN);

        var cte = simpleIdentifier
            .And(cteColumnList.Optional())
            .AndSkip(AS)
            .And(Between(LPAREN, unionStatementList, RPAREN))
            .Then(result =>
            {
                var (name, columns, query) = result;
                return new CommonTableExpression(name.ToString(), query, columns.OrSome(null));
            });

        var cteList = Separated(COMMA, cte);
        var withClause = WITH.And(cteList)
            .Then(x => new WithClause(x.Item2));

        // UNION
        var unionClause = UNION.And(ALL.Optional())
            .Then(x => new UnionClause(x.Item2.HasValue));

        // Statement
        var statement = withClause.Optional().And(selectStatement)
            .Then(result =>
            {
                var (with, select) = result;
                return new Statement(select, with.OrSome(null));
            });

        var unionStatement = statement.And(unionClause.Optional())
            .Then(result =>
            {
                var (stmt, union) = result;
                return new UnionStatement(stmt, union.OrSome(null));
            });

        unionStatementList.Parser = OneOrMany(unionStatement);

        // Statement line
        var statementLine = unionStatementList.AndSkip(SEMICOLON.Optional())
            .Then(x => new StatementLine(x));

        // Statement list
        var statementList = OneOrMany(statementLine)
            .Then(statements => new StatementList(statements));

        Statements = statementList;
    }

    public static StatementList? Parse(string input)
    {
        if (TryParse(input, out var result, out var error))
        {
            return result;
        }

        return null;
    }

    public static bool TryParse(string input, out StatementList? result, out ParseError? error)
    {
        return Statements.TryParse(input, out result, out error);
    }
}
