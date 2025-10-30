#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Queries.Sql;

// Base interface
public interface ISqlNode
{
}

// Statements
public class StatementList : ISqlNode
{
    public IReadOnlyList<StatementLine> Statements { get; }

    public StatementList(IReadOnlyList<StatementLine> statements)
    {
        Statements = statements;
    }
}

public class StatementLine : ISqlNode
{
    public IReadOnlyList<UnionStatement> UnionStatements { get; }

    public StatementLine(IReadOnlyList<UnionStatement> unionStatements)
    {
        UnionStatements = unionStatements;
    }
}

public class UnionStatement : ISqlNode
{
    public Statement Statement { get; }
    public UnionClause? UnionClause { get; }

    public UnionStatement(Statement statement, UnionClause? unionClause = null)
    {
        Statement = statement;
        UnionClause = unionClause;
    }
}

public class UnionClause : ISqlNode
{
    public bool IsAll { get; }

    public UnionClause(bool isAll = false)
    {
        IsAll = isAll;
    }
}

public class Statement : ISqlNode
{
    public WithClause? WithClause { get; }
    public SelectStatement SelectStatement { get; }

    public Statement(SelectStatement selectStatement, WithClause? withClause = null)
    {
        SelectStatement = selectStatement;
        WithClause = withClause;
    }
}

public class WithClause : ISqlNode
{
    public IReadOnlyList<CommonTableExpression> CTEs { get; }

    public WithClause(IReadOnlyList<CommonTableExpression> ctes)
    {
        CTEs = ctes;
    }
}

public class CommonTableExpression : ISqlNode
{
    public string Name { get; }
    public IReadOnlyList<string>? ColumnNames { get; }
    public IReadOnlyList<UnionStatement> Query { get; }

    public CommonTableExpression(string name, IReadOnlyList<UnionStatement> query, IReadOnlyList<string>? columnNames = null)
    {
        Name = name;
        Query = query;
        ColumnNames = columnNames;
    }
}

public class SelectStatement : ISqlNode
{
    public SelectRestriction Restriction { get; }
    public IReadOnlyList<ColumnItem> ColumnItemList { get; }
    public FromClause? FromClause { get; }
    public WhereClause? WhereClause { get; }
    public GroupByClause? GroupByClause { get; }
    public HavingClause? HavingClause { get; }
    public OrderByClause? OrderByClause { get; }
    public LimitClause? LimitClause { get; }
    public OffsetClause? OffsetClause { get; }

    public SelectStatement(
        IReadOnlyList<ColumnItem> columnItemList,
        SelectRestriction? restriction = null,
        FromClause? fromClause = null,
        WhereClause? whereClause = null,
        GroupByClause? groupByClause = null,
        HavingClause? havingClause = null,
        OrderByClause? orderByClause = null,
        LimitClause? limitClause = null,
        OffsetClause? offsetClause = null)
    {
        ColumnItemList = columnItemList;
        Restriction = restriction ?? SelectRestriction.NotSpecified;
        FromClause = fromClause;
        WhereClause = whereClause;
        GroupByClause = groupByClause;
        HavingClause = havingClause;
        OrderByClause = orderByClause;
        LimitClause = limitClause;
        OffsetClause = offsetClause;
    }
}

public enum SelectRestriction
{
    NotSpecified,
    All,
    Distinct
}

public class ColumnItem : ISqlNode
{
    public ColumnSource Source { get; }
    public Identifier? Alias { get; }

    public ColumnItem(ColumnSource source, Identifier? alias = null)
    {
        Source = source;
        Alias = alias;
    }
}

public abstract class ColumnSource : ISqlNode
{
}

public class ColumnSourceIdentifier : ColumnSource
{
    public Identifier Identifier { get; }

    public ColumnSourceIdentifier(Identifier identifier)
    {
        Identifier = identifier;
    }
}

public class ColumnSourceFunction : ColumnSource
{
    public FunctionCall FunctionCall { get; }
    public OverClause? OverClause { get; }

    public ColumnSourceFunction(FunctionCall functionCall, OverClause? overClause = null)
    {
        FunctionCall = functionCall;
        OverClause = overClause;
    }
}

// Clauses
public class FromClause : ISqlNode
{
    public IReadOnlyList<TableSource> TableSources { get; }
    public IReadOnlyList<JoinStatement>? Joins { get; }

    public FromClause(IReadOnlyList<TableSource> tableSources, IReadOnlyList<JoinStatement>? joins = null)
    {
        TableSources = tableSources;
        Joins = joins;
    }
}

public abstract class TableSource : ISqlNode
{
}

public class TableSourceItem : TableSource
{
    public Identifier Identifier { get; }
    public Identifier? Alias { get; }

    public TableSourceItem(Identifier identifier, Identifier? alias = null)
    {
        Identifier = identifier;
        Alias = alias;
    }
}

public class TableSourceSubQuery : TableSource
{
    public IReadOnlyList<UnionStatement> Query { get; }
    public string Alias { get; }

    public TableSourceSubQuery(IReadOnlyList<UnionStatement> query, string alias)
    {
        Query = query;
        Alias = alias;
    }
}

public class JoinStatement : ISqlNode
{
    public JoinKind? JoinKind { get; }
    public IReadOnlyList<TableSourceItem> Tables { get; }
    public Expression Conditions { get; }

    public JoinStatement(IReadOnlyList<TableSourceItem> tables, Expression conditions, JoinKind? joinKind = null)
    {
        Tables = tables;
        Conditions = conditions;
        JoinKind = joinKind;
    }
}

public enum JoinKind
{
    None,
    Inner,
    Left,
    Right
}

public class WhereClause : ISqlNode
{
    public Expression Expression { get; }

    public WhereClause(Expression expression)
    {
        Expression = expression;
    }
}

public class GroupByClause : ISqlNode
{
    public IReadOnlyList<ColumnSource> Columns { get; }

    public GroupByClause(IReadOnlyList<ColumnSource> columns)
    {
        Columns = columns;
    }
}

public class HavingClause : ISqlNode
{
    public Expression Expression { get; }

    public HavingClause(Expression expression)
    {
        Expression = expression;
    }
}

public class OrderByClause : ISqlNode
{
    public IReadOnlyList<OrderByItem> Items { get; }

    public OrderByClause(IReadOnlyList<OrderByItem> items)
    {
        Items = items;
    }
}

public class OrderByItem : ISqlNode
{
    public Identifier Identifier { get; }
    public OrderDirection Direction { get; }

    public OrderByItem(Identifier identifier, OrderDirection direction)
    {
        Identifier = identifier;
        Direction = direction;
    }
}

public enum OrderDirection
{
    NotSpecified,
    Asc,
    Desc
}

public class LimitClause : ISqlNode
{
    public Expression Expression { get; }

    public LimitClause(Expression expression)
    {
        Expression = expression;
    }
}

public class OffsetClause : ISqlNode
{
    public Expression Expression { get; }

    public OffsetClause(Expression expression)
    {
        Expression = expression;
    }
}

public class OverClause : ISqlNode
{
    public PartitionByClause? PartitionBy { get; }
    public OrderByClause? OrderBy { get; }

    public OverClause(PartitionByClause? partitionBy = null, OrderByClause? orderBy = null)
    {
        PartitionBy = partitionBy;
        OrderBy = orderBy;
    }
}

public class PartitionByClause : ISqlNode
{
    public IReadOnlyList<ColumnItem> Columns { get; }

    public PartitionByClause(IReadOnlyList<ColumnItem> columns)
    {
        Columns = columns;
    }
}

// Expressions
public abstract class Expression : ISqlNode
{
}

public class BinaryExpression : Expression
{
    public Expression Left { get; }
    public BinaryOperator Operator { get; }
    public Expression Right { get; }

    public BinaryExpression(Expression left, BinaryOperator op, Expression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
}

public enum BinaryOperator
{
    // Arithmetic
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    // Bitwise
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    // Comparison
    Equal,
    NotEqual,
    NotEqualAlt,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    NotGreaterThan,
    NotLessThan,
    // Logical
    And,
    Or,
    Like,
    NotLike
}

public class UnaryExpression : Expression
{
    public UnaryOperator Operator { get; }
    public Expression Expression { get; }

    public UnaryExpression(UnaryOperator op, Expression expression)
    {
        Operator = op;
        Expression = expression;
    }
}

public enum UnaryOperator
{
    Not,
    Plus,
    Minus,
    BitwiseNot
}

public class BetweenExpression : Expression
{
    public Expression Expression { get; }
    public bool IsNot { get; }
    public Expression Lower { get; }
    public Expression Upper { get; }

    public BetweenExpression(Expression expression, Expression lower, Expression upper, bool isNot = false)
    {
        Expression = expression;
        Lower = lower;
        Upper = upper;
        IsNot = isNot;
    }
}

public class InExpression : Expression
{
    public Expression Expression { get; }
    public bool IsNot { get; }
    public IReadOnlyList<Expression> Values { get; }

    public InExpression(Expression expression, IReadOnlyList<Expression> values, bool isNot = false)
    {
        Expression = expression;
        Values = values;
        IsNot = isNot;
    }
}

public class IdentifierExpression : Expression
{
    public Identifier Identifier { get; }

    public IdentifierExpression(Identifier identifier)
    {
        Identifier = identifier;
    }
}

public class LiteralExpression<T> : Expression
{
    public T Value { get; }

    public LiteralExpression(T value)
    {
        Value = value;
    }
}

public class FunctionCall : Expression
{
    public Identifier Name { get; }
    public FunctionArguments Arguments { get; }

    public FunctionCall(Identifier name, FunctionArguments arguments)
    {
        Name = name;
        Arguments = arguments;
    }
}

public abstract class FunctionArguments : ISqlNode
{
}

public class StarArgument : FunctionArguments
{
}

public class SelectStatementArgument : FunctionArguments
{
    public SelectStatement SelectStatement { get; }

    public SelectStatementArgument(SelectStatement selectStatement)
    {
        SelectStatement = selectStatement;
    }
}

public class ExpressionListArguments : FunctionArguments
{
    public IReadOnlyList<Expression> Expressions { get; }

    public ExpressionListArguments(IReadOnlyList<Expression> expressions)
    {
        Expressions = expressions;
    }
}

public class TupleExpression : Expression
{
    public IReadOnlyList<Expression> Expressions { get; }

    public TupleExpression(IReadOnlyList<Expression> expressions)
    {
        Expressions = expressions;
    }
}

public class ParenthesizedSelectStatement : Expression
{
    public SelectStatement SelectStatement { get; }

    public ParenthesizedSelectStatement(SelectStatement selectStatement)
    {
        SelectStatement = selectStatement;
    }
}

public class ParameterExpression : Expression
{
    public Identifier Name { get; }
    public Expression? DefaultValue { get; }

    public ParameterExpression(Identifier name, Expression? defaultValue = null)
    {
        Name = name;
        DefaultValue = defaultValue;
    }
}

// Identifiers
public class Identifier : ISqlNode
{
    public IReadOnlyList<string> Parts { get; }

    public Identifier(IReadOnlyList<string> parts)
    {
        Parts = parts;
    }

    public Identifier(string name) : this(new[] { name })
    {
    }

    public Identifier(params string[] parts)
    {
        Parts = parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
    }

    public override string ToString()
    {
        return string.Join(".", Parts);
    }
}
