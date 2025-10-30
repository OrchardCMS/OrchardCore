using System.Text;
using YesSql;

namespace OrchardCore.Queries.Sql;

public class SqlTranslator
{
    private readonly string _schema;
    private readonly IDictionary<string, object> _parameters;
    private readonly ISqlDialect _dialect;
    private readonly string _tablePrefix;
    private HashSet<string> _tableAliases;
    private HashSet<string> _ctes;
    private StringBuilder _builder;

    public SqlTranslator(string schema, ISqlDialect dialect, string tablePrefix, IDictionary<string, object> parameters)
    {
        _schema = schema;
        _dialect = dialect;
        _tablePrefix = tablePrefix;
        _parameters = parameters ?? new Dictionary<string, object>();
        _builder = new StringBuilder();
    }

    public string Translate(StatementList statementList)
    {
        // First, collect all table aliases and CTE names
        CollectAliasesAndCtes(statementList);

        var statementsBuilder = new StringBuilder();

        foreach (var statementLine in statementList.Statements)
        {
            TranslateStatementLine(statementsBuilder, statementLine);
        }

        statementsBuilder.Append(';');
        return statementsBuilder.ToString();
    }

    private void CollectAliasesAndCtes(StatementList statementList)
    {
        _tableAliases = new HashSet<string>();
        _ctes = new HashSet<string>();

        foreach (var statementLine in statementList.Statements)
        {
            foreach (var unionStatement in statementLine.UnionStatements)
            {
                var statement = unionStatement.Statement;
                
                // Collect CTE names
                if (statement.WithClause != null)
                {
                    foreach (var cte in statement.WithClause.CTEs)
                    {
                        _ctes.Add(cte.Name);
                    }
                }

                // Collect table aliases
                CollectTableAliases(statement.SelectStatement);
            }
        }
    }

    private void CollectTableAliases(SelectStatement selectStatement)
    {
        if (selectStatement.FromClause != null)
        {
            foreach (var tableSource in selectStatement.FromClause.TableSources)
            {
                if (tableSource is TableSourceItem item && item.Alias != null)
                {
                    _tableAliases.Add(item.Alias.ToString());
                }
                else if (tableSource is TableSourceSubQuery subQuery)
                {
                    _tableAliases.Add(subQuery.Alias);
                }
            }

            if (selectStatement.FromClause.Joins != null)
            {
                foreach (var join in selectStatement.FromClause.Joins)
                {
                    foreach (var table in join.Tables)
                    {
                        if (table.Alias != null)
                        {
                            _tableAliases.Add(table.Alias.ToString());
                        }
                    }
                }
            }
        }
    }

    private void TranslateStatementLine(StringBuilder builder, StatementLine statementLine)
    {
        foreach (var unionStatement in statementLine.UnionStatements)
        {
            TranslateUnionStatement(builder, unionStatement);
        }
    }

    private void TranslateUnionStatement(StringBuilder builder, UnionStatement unionStatement)
    {
        var statement = unionStatement.Statement;

        // WITH clause (CTEs)
        if (statement.WithClause != null)
        {
            TranslateWithClause(builder, statement.WithClause);
        }

        // SELECT statement
        TranslateSelectStatement(builder, statement.SelectStatement);

        // UNION clause
        if (unionStatement.UnionClause != null)
        {
            builder.Append(' ');
            builder.Append("UNION");
            if (unionStatement.UnionClause.IsAll)
            {
                builder.Append(" ALL");
            }
            builder.Append(' ');
        }
    }

    private void TranslateWithClause(StringBuilder builder, WithClause withClause)
    {
        builder.Append("WITH ");

        for (var i = 0; i < withClause.CTEs.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            var cte = withClause.CTEs[i];
            builder.Append(cte.Name);

            if (cte.ColumnNames != null && cte.ColumnNames.Count > 0)
            {
                builder.Append('(');
                for (var j = 0; j < cte.ColumnNames.Count; j++)
                {
                    if (j > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(cte.ColumnNames[j]);
                }
                builder.Append(')');
            }

            builder.Append(" AS (");
            
            for (var j = 0; j < cte.Query.Count; j++)
            {
                TranslateUnionStatement(builder, cte.Query[j]);
            }
            
            builder.Append(')');
        }

        builder.Append(' ');
    }

    private void TranslateSelectStatement(StringBuilder builder, SelectStatement selectStatement)
    {
        var sqlBuilder = _dialect.CreateBuilder(_tablePrefix);

        // SELECT restriction (DISTINCT/ALL)
        if (selectStatement.Restriction == SelectRestriction.Distinct)
        {
            sqlBuilder.Distinct();
        }
        else if (selectStatement.Restriction == SelectRestriction.All)
        {
            // ALL is the default, no need to add it
        }

        // SELECT clause
        sqlBuilder.Select();
        _builder.Clear();
        TranslateColumnItemList(_builder, selectStatement.ColumnItemList);
        sqlBuilder.Selector(_builder.ToString());

        // FROM clause
        if (selectStatement.FromClause != null)
        {
            _builder.Clear();
            TranslateFromClause(_builder, selectStatement.FromClause);
            sqlBuilder.From(_builder.ToString());
        }

        // WHERE clause
        if (selectStatement.WhereClause != null)
        {
            _builder.Clear();
            TranslateExpression(_builder, selectStatement.WhereClause.Expression);
            sqlBuilder.WhereAnd(_builder.ToString());
        }

        // GROUP BY clause
        if (selectStatement.GroupByClause != null)
        {
            _builder.Clear();
            TranslateGroupByClause(_builder, selectStatement.GroupByClause);
            sqlBuilder.GroupBy(_builder.ToString());
        }

        // HAVING clause
        if (selectStatement.HavingClause != null)
        {
            _builder.Clear();
            TranslateExpression(_builder, selectStatement.HavingClause.Expression);
            sqlBuilder.Having(_builder.ToString());
        }

        // ORDER BY clause
        if (selectStatement.OrderByClause != null)
        {
            _builder.Clear();
            TranslateOrderByClause(_builder, selectStatement.OrderByClause);
            sqlBuilder.OrderBy(_builder.ToString());
        }

        // LIMIT clause
        if (selectStatement.LimitClause != null)
        {
            _builder.Clear();
            TranslateExpression(_builder, selectStatement.LimitClause.Expression);
            sqlBuilder.Take(_builder.ToString());
        }

        // OFFSET clause
        if (selectStatement.OffsetClause != null)
        {
            _builder.Clear();
            TranslateExpression(_builder, selectStatement.OffsetClause.Expression);
            sqlBuilder.Skip(_builder.ToString());
        }

        builder.Append(sqlBuilder.ToSqlString());
    }

    private void TranslateColumnItemList(StringBuilder builder, IReadOnlyList<ColumnItem> columnItems)
    {
        // Check if it's SELECT *
        if (columnItems.Count == 1 && 
            columnItems[0].Source is ColumnSourceIdentifier idSource &&
            idSource.Identifier.Parts.Count == 1 &&
            idSource.Identifier.Parts[0] == "*")
        {
            builder.Append('*');
            return;
        }

        for (var i = 0; i < columnItems.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            TranslateColumnItem(builder, columnItems[i]);
        }
    }

    private void TranslateColumnItem(StringBuilder builder, ColumnItem columnItem)
    {
        TranslateColumnSource(builder, columnItem.Source);

        if (columnItem.Alias != null)
        {
            builder.Append(" AS ");
            builder.Append(columnItem.Alias.ToString());
        }
    }

    private void TranslateColumnSource(StringBuilder builder, ColumnSource columnSource)
    {
        if (columnSource is ColumnSourceIdentifier identifierSource)
        {
            TranslateIdentifierInSelectContext(builder, identifierSource.Identifier);
        }
        else if (columnSource is ColumnSourceFunction functionSource)
        {
            TranslateFunctionCall(builder, functionSource.FunctionCall);

            if (functionSource.OverClause != null)
            {
                TranslateOverClause(builder, functionSource.OverClause);
            }
        }
    }

    private void TranslateOverClause(StringBuilder builder, OverClause overClause)
    {
        builder.Append(" OVER (");

        if (overClause.PartitionBy != null)
        {
            builder.Append("PARTITION BY ");
            for (var i = 0; i < overClause.PartitionBy.Columns.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }
                TranslateColumnItem(builder, overClause.PartitionBy.Columns[i]);
            }
        }

        if (overClause.OrderBy != null)
        {
            if (overClause.PartitionBy != null)
            {
                builder.Append(' ');
            }
            builder.Append("ORDER BY ");
            TranslateOrderByList(builder, overClause.OrderBy.Items);
        }

        builder.Append(')');
    }

    private void TranslateFromClause(StringBuilder builder, FromClause fromClause)
    {
        for (var i = 0; i < fromClause.TableSources.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            TranslateTableSource(builder, fromClause.TableSources[i]);
        }

        // JOIN clauses
        if (fromClause.Joins != null)
        {
            foreach (var join in fromClause.Joins)
            {
                TranslateJoinStatement(builder, join);
            }
        }
    }

    private void TranslateTableSource(StringBuilder builder, TableSource tableSource)
    {
        if (tableSource is TableSourceItem item)
        {
            TranslateIdentifierInFromContext(builder, item.Identifier);

            if (item.Alias != null)
            {
                builder.Append(" AS ");
                builder.Append(item.Alias.ToString());
            }
        }
        else if (tableSource is TableSourceSubQuery subQuery)
        {
            builder.Append('(');
            
            for (var i = 0; i < subQuery.Query.Count; i++)
            {
                TranslateUnionStatement(builder, subQuery.Query[i]);
            }
            
            builder.Append(") AS ");
            builder.Append(subQuery.Alias);
        }
    }

    private void TranslateJoinStatement(StringBuilder builder, JoinStatement join)
    {
        builder.Append(' ');

        if (join.JoinKind == JoinKind.Inner)
        {
            builder.Append("INNER ");
        }
        else if (join.JoinKind == JoinKind.Left)
        {
            builder.Append("LEFT ");
        }
        else if (join.JoinKind == JoinKind.Right)
        {
            builder.Append("RIGHT ");
        }

        builder.Append("JOIN ");

        for (var i = 0; i < join.Tables.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            var table = join.Tables[i];
            TranslateIdentifierInFromContext(builder, table.Identifier);

            if (table.Alias != null)
            {
                builder.Append(" AS ");
                builder.Append(table.Alias.ToString());
            }
        }

        builder.Append(" ON ");
        TranslateExpression(builder, join.Conditions);
    }

    private void TranslateGroupByClause(StringBuilder builder, GroupByClause groupByClause)
    {
        for (var i = 0; i < groupByClause.Columns.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            TranslateColumnSource(builder, groupByClause.Columns[i]);
        }
    }

    private void TranslateOrderByClause(StringBuilder builder, OrderByClause orderByClause)
    {
        TranslateOrderByList(builder, orderByClause.Items);
    }

    private void TranslateOrderByList(StringBuilder builder, IReadOnlyList<OrderByItem> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            var item = items[i];

            // Check for RANDOM() special case
            if (item.Identifier.Parts.Count == 1 && 
                item.Identifier.Parts[0].Equals("RANDOM", StringComparison.OrdinalIgnoreCase))
            {
                builder.Append(_dialect.RandomOrderByClause);
                continue;
            }

            TranslateIdentifierInSelectContext(builder, item.Identifier);

            if (item.Direction == OrderDirection.Asc)
            {
                builder.Append(" ASC");
            }
            else if (item.Direction == OrderDirection.Desc)
            {
                builder.Append(" DESC");
            }
        }
    }

    private void TranslateExpression(StringBuilder builder, Expression expression)
    {
        switch (expression)
        {
            case BinaryExpression binary:
                TranslateBinaryExpression(builder, binary);
                break;
            case UnaryExpression unary:
                TranslateUnaryExpression(builder, unary);
                break;
            case BetweenExpression between:
                TranslateBetweenExpression(builder, between);
                break;
            case InExpression inExpr:
                TranslateInExpression(builder, inExpr);
                break;
            case IdentifierExpression identifier:
                TranslateIdentifierInSelectContext(builder, identifier.Identifier);
                break;
            case LiteralExpression<bool> boolLiteral:
                builder.Append(_dialect.GetSqlValue(boolLiteral.Value));
                break;
            case LiteralExpression<string> stringLiteral:
                builder.Append(_dialect.GetSqlValue(stringLiteral.Value));
                break;
            case LiteralExpression<decimal> decimalLiteral:
                builder.Append(_dialect.GetSqlValue(decimalLiteral.Value));
                break;
            case FunctionCall functionCall:
                TranslateFunctionCall(builder, functionCall);
                break;
            case TupleExpression tuple:
                TranslateTupleExpression(builder, tuple);
                break;
            case ParenthesizedSelectStatement parSelect:
                TranslateParenthesizedSelectStatement(builder, parSelect);
                break;
            case ParameterExpression parameter:
                TranslateParameterExpression(builder, parameter);
                break;
            default:
                throw new SqlParserException($"Unsupported expression type: {expression.GetType().Name}");
        }
    }

    private void TranslateBinaryExpression(StringBuilder builder, BinaryExpression binary)
    {
        TranslateExpression(builder, binary.Left);
        builder.Append(' ');
        builder.Append(GetBinaryOperatorString(binary.Operator));
        builder.Append(' ');
        TranslateExpression(builder, binary.Right);
    }

    private string GetBinaryOperatorString(BinaryOperator op)
    {
        return op switch
        {
            BinaryOperator.Add => "+",
            BinaryOperator.Subtract => "-",
            BinaryOperator.Multiply => "*",
            BinaryOperator.Divide => "/",
            BinaryOperator.Modulo => "%",
            BinaryOperator.BitwiseAnd => "&",
            BinaryOperator.BitwiseOr => "|",
            BinaryOperator.BitwiseXor => "^",
            BinaryOperator.Equal => "=",
            BinaryOperator.NotEqual => "<>",
            BinaryOperator.NotEqualAlt => "!=",
            BinaryOperator.GreaterThan => ">",
            BinaryOperator.LessThan => "<",
            BinaryOperator.GreaterThanOrEqual => ">=",
            BinaryOperator.LessThanOrEqual => "<=",
            BinaryOperator.NotGreaterThan => "!>",
            BinaryOperator.NotLessThan => "!<",
            BinaryOperator.And => "AND",
            BinaryOperator.Or => "OR",
            BinaryOperator.Like => "LIKE",
            BinaryOperator.NotLike => "NOT LIKE",
            _ => throw new SqlParserException($"Unsupported binary operator: {op}")
        };
    }

    private void TranslateUnaryExpression(StringBuilder builder, UnaryExpression unary)
    {
        builder.Append(GetUnaryOperatorString(unary.Operator));
        TranslateExpression(builder, unary.Expression);
    }

    private string GetUnaryOperatorString(UnaryOperator op)
    {
        return op switch
        {
            UnaryOperator.Not => "NOT ",
            UnaryOperator.Plus => "+",
            UnaryOperator.Minus => "-",
            UnaryOperator.BitwiseNot => "~",
            _ => throw new SqlParserException($"Unsupported unary operator: {op}")
        };
    }

    private void TranslateBetweenExpression(StringBuilder builder, BetweenExpression between)
    {
        TranslateExpression(builder, between.Expression);
        builder.Append(' ');
        
        if (between.IsNot)
        {
            builder.Append("NOT ");
        }
        
        builder.Append("BETWEEN ");
        TranslateExpression(builder, between.Lower);
        builder.Append(" AND ");
        TranslateExpression(builder, between.Upper);
    }

    private void TranslateInExpression(StringBuilder builder, InExpression inExpr)
    {
        TranslateExpression(builder, inExpr.Expression);
        builder.Append(' ');
        
        if (inExpr.IsNot)
        {
            builder.Append("NOT ");
        }
        
        builder.Append("IN (");

        for (var i = 0; i < inExpr.Values.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }
            TranslateExpression(builder, inExpr.Values[i]);
        }

        builder.Append(')');
    }

    private void TranslateTupleExpression(StringBuilder builder, TupleExpression tuple)
    {
        builder.Append('(');
        for (var i = 0; i < tuple.Expressions.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }
            TranslateExpression(builder, tuple.Expressions[i]);
        }
        builder.Append(')');
    }

    private void TranslateParenthesizedSelectStatement(StringBuilder builder, ParenthesizedSelectStatement parSelect)
    {
        builder.Append('(');
        TranslateSelectStatement(builder, parSelect.SelectStatement);
        builder.Append(')');
    }

    private void TranslateParameterExpression(StringBuilder builder, ParameterExpression parameter)
    {
        var name = parameter.Name.ToString();
        builder.Append("@" + name);

        if (!_parameters.ContainsKey(name))
        {
            if (parameter.DefaultValue != null)
            {
                // Extract the default value
                var defaultValue = ExtractLiteralValue(parameter.DefaultValue);
                _parameters[name] = defaultValue;
            }
            else
            {
                throw new SqlParserException($"Missing parameter: {name}");
            }
        }
    }

    private object ExtractLiteralValue(Expression expression)
    {
        return expression switch
        {
            LiteralExpression<bool> boolLiteral => boolLiteral.Value,
            LiteralExpression<string> stringLiteral => stringLiteral.Value,
            LiteralExpression<decimal> decimalLiteral => decimalLiteral.Value,
            _ => throw new SqlParserException("Unsupported default parameter value type")
        };
    }

    private void TranslateFunctionCall(StringBuilder builder, FunctionCall functionCall)
    {
        var funcName = functionCall.Name.ToString();
        var arguments = TranslateFunctionArguments(functionCall.Arguments);
        builder.Append(_dialect.RenderMethod(funcName, arguments));
    }

    private string[] TranslateFunctionArguments(FunctionArguments arguments)
    {
        return arguments switch
        {
            StarArgument => new string[] { "*" },
            SelectStatementArgument selectArg => new string[] { TranslateSelectStatementToString(selectArg.SelectStatement) },
            ExpressionListArguments exprList => TranslateExpressionListToArray(exprList.Expressions),
            _ => throw new SqlParserException($"Unsupported function argument type: {arguments.GetType().Name}")
        };
    }

    private string TranslateSelectStatementToString(SelectStatement selectStatement)
    {
        var tempBuilder = new StringBuilder();
        TranslateSelectStatement(tempBuilder, selectStatement);
        return tempBuilder.ToString();
    }

    private string[] TranslateExpressionListToArray(IReadOnlyList<Expression> expressions)
    {
        var result = new string[expressions.Count];
        for (var i = 0; i < expressions.Count; i++)
        {
            var tempBuilder = new StringBuilder();
            TranslateExpression(tempBuilder, expressions[i]);
            result[i] = tempBuilder.ToString();
        }
        return result;
    }

    private void TranslateIdentifierInSelectContext(StringBuilder builder, Identifier identifier)
    {
        // In SELECT context, first part is table name unless it's an alias
        if (identifier.Parts.Count == 1)
        {
            builder.Append(_dialect.QuoteForColumnName(identifier.Parts[0]));
        }
        else
        {
            for (var i = 0; i < identifier.Parts.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append('.');
                }

                if (i == 0 && !_tableAliases.Contains(identifier.Parts[i]))
                {
                    // First part is a table name, needs table prefix
                    builder.Append(_dialect.QuoteForTableName(_tablePrefix + identifier.Parts[i], _schema));
                }
                else
                {
                    // It's an alias or column name
                    if (_tableAliases.Contains(identifier.Parts[i]))
                    {
                        builder.Append(identifier.Parts[i]);
                    }
                    else
                    {
                        builder.Append(_dialect.QuoteForColumnName(identifier.Parts[i]));
                    }
                }
            }
        }
    }

    private void TranslateIdentifierInFromContext(StringBuilder builder, Identifier identifier)
    {
        // In FROM context, identifier is a table name unless it's a CTE
        for (var i = 0; i < identifier.Parts.Count; i++)
        {
            if (i == 0 && !_ctes.Contains(identifier.Parts[i]))
            {
                // It's a table name, add prefix
                builder.Append(_dialect.QuoteForTableName(_tablePrefix + identifier.Parts[i], _schema));
            }
            else
            {
                // It's a CTE name or schema/qualifier
                builder.Append(_dialect.QuoteForColumnName(identifier.Parts[i]));
            }
        }
    }
}
