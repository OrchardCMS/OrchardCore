#nullable enable

using Parlot;
using Parlot.Fluent;

namespace OrchardCore.Queries.Sql;

/// <summary>
/// SQL parser.
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

        // Keywords
        var SELECT = Terms.Keyword("SELECT", caseInsensitive: true);
        var FROM = Terms.Keyword("FROM", caseInsensitive: true);
        var WHERE = Terms.Keyword("WHERE", caseInsensitive: true);
        var AS = Terms.Keyword("AS", caseInsensitive: true);
        var JOIN = Terms.Keyword("JOIN", caseInsensitive: true);
        var INNER = Terms.Keyword("INNER", caseInsensitive: true);
        var LEFT = Terms.Keyword("LEFT", caseInsensitive: true);
        var RIGHT = Terms.Keyword("RIGHT", caseInsensitive: true);
        var ON = Terms.Keyword("ON", caseInsensitive: true);
        var GROUP = Terms.Keyword("GROUP", caseInsensitive: true);
        var BY = Terms.Keyword("BY", caseInsensitive: true);
        var HAVING = Terms.Keyword("HAVING", caseInsensitive: true);
        var ORDER = Terms.Keyword("ORDER", caseInsensitive: true);
        var ASC = Terms.Keyword("ASC", caseInsensitive: true);
        var DESC = Terms.Keyword("DESC", caseInsensitive: true);
        var LIMIT = Terms.Keyword("LIMIT", caseInsensitive: true);
        var OFFSET = Terms.Keyword("OFFSET", caseInsensitive: true);
        var UNION = Terms.Keyword("UNION", caseInsensitive: true);
        var ALL = Terms.Keyword("ALL", caseInsensitive: true);
        var DISTINCT = Terms.Keyword("DISTINCT", caseInsensitive: true);
        var WITH = Terms.Keyword("WITH", caseInsensitive: true);
        var AND = Terms.Keyword("AND", caseInsensitive: true);
        var OR = Terms.Keyword("OR", caseInsensitive: true);
        var NOT = Terms.Keyword("NOT", caseInsensitive: true);
        var BETWEEN = Terms.Keyword("BETWEEN", caseInsensitive: true);
        var IN = Terms.Keyword("IN", caseInsensitive: true);
        var LIKE = Terms.Keyword("LIKE", caseInsensitive: true);
        var TRUE = Terms.Keyword("TRUE", caseInsensitive: true);
        var FALSE = Terms.Keyword("FALSE", caseInsensitive: true);
        var OVER = Terms.Keyword("OVER", caseInsensitive: true);
        var PARTITION = Terms.Keyword("PARTITION", caseInsensitive: true);
        
        // Keywords can't be used as identifiers or function names
        var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SELECT", "FROM", "WHERE", "AS", "JOIN", "INNER", "LEFT", "RIGHT", "ON",
            "GROUP", "BY", "HAVING", "ORDER", "ASC", "DESC", "LIMIT", "OFFSET",
            "UNION", "ALL", "DISTINCT", "WITH", "AND", "OR", "NOT", "BETWEEN",
            "IN", "LIKE", "TRUE", "FALSE", "OVER", "PARTITION",
        };

        // Literals
        var numberLiteral = Terms.Decimal().Then<Expression>(d => d.Scale == 0 ? new LiteralExpression<long>((long)d) : new LiteralExpression<decimal>(d));

        var stringLiteral = Terms.String(StringLiteralQuotes.Single)
            .Then<Expression>(s => new LiteralExpression<string>(s.ToString()));

        var booleanLiteral = TRUE.Then<Expression>(new LiteralExpression<bool>(true))
            .Or(FALSE.Then<Expression>(new LiteralExpression<bool>(false)));

        // Identifiers
        var simpleIdentifier = Terms.Identifier().Then(x => x.ToString())
            .Or(Between(Terms.Char('['), Literals.NoneOf("]"), Terms.Char(']')).Then(x => x.ToString()))
            .Or(Between(Terms.Char('"'), Literals.NoneOf("\""), Terms.Char('"')).Then(x => x.ToString()));

        var identifier = Separated(DOT, simpleIdentifier)
            .Then(parts => new Identifier(parts));

        // Without the keywords check "FROM a WHERE" would interpret "WHERE" as an alias since "AS" is optional
        var identifierNoKeywords = Separated(DOT, simpleIdentifier).When((ctx, parts) => parts.Count > 0 && !keywords.Contains(parts[0]))
            .Then(parts => new Identifier(parts));
            
        // Deferred parsers
        var expression = Deferred<Expression>();
        var selectStatement = Deferred<SelectStatement>();
        var columnItem = Deferred<ColumnItem>();
        var orderByItem = Deferred<OrderByItem>();

        // Expression list
        var expressionList = Separated(COMMA, expression);

        // Function arguments
        var starArg = STAR.Then<FunctionArguments>(_ => StarArgument.Instance);
        var selectArg = selectStatement.Then<FunctionArguments>(s => new SelectStatementArgument(s));
        var exprListArg = expressionList.Then<FunctionArguments>(exprs => new ExpressionListArguments(exprs));
        var emptyArg = Always<FunctionArguments>(EmptyArguments.Instance);
        var functionArgs = starArg.Or(selectArg).Or(exprListArg).Or(emptyArg);

        // Function call
        var functionCall = identifier.And(Between(LPAREN, functionArgs, RPAREN))
            .Then<Expression>(x => new FunctionCall(x.Item1, x.Item2));

        // Tuple
        var tuple = Between(LPAREN, expressionList, RPAREN)
            .Then<Expression>(exprs => new TupleExpression(exprs));

        // Parenthesized select
        var parSelectStatement = Between(LPAREN, selectStatement, RPAREN)
            .Then<Expression>(s => new ParenthesizedSelectStatement(s));

        // Basic term
        var identifierExpr = identifier.Then<Expression>(id => new IdentifierExpression(id));

        var termNoParameter = functionCall
            .Or(parSelectStatement)
            .Or(tuple)
            .Or(booleanLiteral)
            .Or(stringLiteral)
            .Or(numberLiteral)
            .Or(identifierExpr)
            ;

        // Parameter - keywords are allowed as parameter names
        var parameter = AT.SkipAnd(identifier).And(Literals.Char(':').SkipAnd(termNoParameter).Optional()).Then<Expression>(x => new ParameterExpression(x.Item1, x.Item2.HasValue ? x.Item2.Value : null));

        var term = termNoParameter.Or(parameter);

        // Unary expressions
        var unaryMinus = Terms.Char('-').And(term).Then<Expression>(x => new UnaryExpression(UnaryOperator.Minus, x.Item2));
        var unaryPlus = Terms.Char('+').And(term).Then<Expression>(x => new UnaryExpression(UnaryOperator.Plus, x.Item2));
        var unaryNot = NOT.And(term).Then<Expression>(x => new UnaryExpression(UnaryOperator.Not, x.Item2));
        var unaryBitwiseNot = Terms.Char('~').And(term).Then<Expression>(x => new UnaryExpression(UnaryOperator.BitwiseNot, x.Item2));

        var unaryExpr = unaryMinus.Or(unaryPlus).Or(unaryNot).Or(unaryBitwiseNot);
        var primary = unaryExpr.Or(term);

        // Binary operators
        var notLike = NOT.AndSkip(LIKE);
        var likeOp = notLike.Or(LIKE);

        // Build expression with proper precedence
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

        var inExpr = andExpr.And(NOT.Optional()).AndSkip(IN).AndSkip(LPAREN).And(functionArgs).AndSkip(RPAREN)
            .Then<Expression>(result =>
            {
                var (expr, notKeyword, values) = result;
                return new InExpression(expr, values, notKeyword.HasValue);
            });

        expression.Parser = betweenExpr.Or(inExpr).Or(orExpr);

        // Column source
        var columnSourceId = identifier.Then<ColumnSource>(id => new ColumnSourceIdentifier(id));

        // e.g. SELECT 1, 'text', etc.
        var columnSourceValue = primary.Then<ColumnSource>(x => new ColumnSourceValue(x));

        // Deferred for OVER clause components
        var columnItemList = Separated(COMMA, columnItem.Or(STAR.Then(new ColumnItem(new ColumnSourceIdentifier(Identifier.STAR), null))));
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

        var columnSource = columnSourceFunc.Or(columnSourceId).Or(columnSourceValue);

        // Column item with alias
        var columnAlias = AS.Optional().SkipAnd(identifierNoKeywords);

        columnItem.Parser = columnSource.And(columnAlias.Optional())
            .Then(result =>
            {
                var (source, alias) = result;
                return new ColumnItem(source, alias.OrSome(null));
            });

        // Table source
        var tableAlias = AS.Optional().SkipAnd(identifierNoKeywords);

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
                var (kind, tables, conditions) = result;
                return new JoinStatement(tables, conditions, kind);
            });

        var joins = ZeroOrMany(joinStatement);

        // FROM clause
        var fromClause = FROM.SkipAnd(tableSourceList).And(joins)
            .Then(result =>
            {
                var (tables, joinList) = result;
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

        orderByItem.Parser =
            identifier.And(Between(LPAREN, functionArgs, RPAREN))
                .Then(result =>
                {
                    var (id, arguments) = result;
                    return new OrderByItem(id, arguments, OrderDirection.NotSpecified);
                }).Or(
            identifier.And(orderDirection.Optional())
                .Then(result =>
                {
                    var (id, dir) = result;
                    return new OrderByItem(id, null, dir.OrSome(OrderDirection.NotSpecified));
                }));

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
        var columnNames = Separated(COMMA, simpleIdentifier);

        var cteColumnList = Between(LPAREN, columnNames, RPAREN);

        var cte = simpleIdentifier
            .And(cteColumnList.Optional())
            .AndSkip(AS)
            .And(Between(LPAREN, unionStatementList, RPAREN))
            .Then(result =>
            {
                var (name, columns, query) = result;
                return new CommonTableExpression(name, query, columns.OrSome(null));
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
        var statementList = ZeroOrMany(statementLine)
            .Then(statements => new StatementList(statements))
            .AndSkip(Terms.WhiteSpace().Optional()) // allow trailing whitespace
            .Eof();

        Statements = statementList.WithComments(comments =>
        {
            comments
                .WithWhiteSpaceOrNewLine()
                .WithSingleLine("--")
                .WithMultiLine("/*", "*/")
                ;
        });
    }

    public static StatementList? Parse(string input)
    {
        if (TryParse(input, out var result, out var _))
        {
            return result;
        }

        return null;
    }

    public static bool TryParse(string input, out StatementList? result, out ParseError? error)
    {
        var context = new ParseContext(new Scanner(input), disableLoopDetection: true);
        return Statements.TryParse(context, out result, out error);
    }
}
