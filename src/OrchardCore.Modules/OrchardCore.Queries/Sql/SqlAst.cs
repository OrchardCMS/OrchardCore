#nullable enable

namespace OrchardCore.Queries.Sql;

// Base interface
public interface ISqlNode
{
}

// Statements
public sealed class StatementList : ISqlNode
{
    public IReadOnlyList<StatementLine> Statements { get; }

    public StatementList(IReadOnlyList<StatementLine> statements)
    {
        Statements = statements;
    }
}

public sealed class StatementLine : ISqlNode
{
    public IReadOnlyList<UnionStatement> UnionStatements { get; }

    public StatementLine(IReadOnlyList<UnionStatement> unionStatements)
    {
        UnionStatements = unionStatements;
    }
}

public sealed class UnionStatement : ISqlNode
{
    public Statement Statement { get; }
    public UnionClause? UnionClause { get; }

    public UnionStatement(Statement statement, UnionClause? unionClause = null)
    {
        Statement = statement;
        UnionClause = unionClause;
    }
}

public sealed class UnionClause : ISqlNode
{
    public bool IsAll { get; }

    public UnionClause(bool isAll = false)
    {
        IsAll = isAll;
    }
}

public sealed class Statement : ISqlNode
{
    public WithClause? WithClause { get; }
    public SelectStatement SelectStatement { get; }

    public Statement(SelectStatement selectStatement, WithClause? withClause = null)
    {
        SelectStatement = selectStatement;
        WithClause = withClause;
    }
}

public sealed class WithClause : ISqlNode
{
    public IReadOnlyList<CommonTableExpression> CTEs { get; }

    public WithClause(IReadOnlyList<CommonTableExpression> ctes)
    {
        CTEs = ctes;
    }
}

public sealed class CommonTableExpression : ISqlNode
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

public sealed class SelectStatement : ISqlNode
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
    Distinct,
}

public sealed class ColumnItem : ISqlNode
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

public sealed class ColumnSourceIdentifier : ColumnSource
{
    public Identifier Identifier { get; }

    public ColumnSourceIdentifier(Identifier identifier)
    {
        Identifier = identifier;
    }
}

public sealed class ColumnSourceFunction : ColumnSource
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
public sealed class FromClause : ISqlNode
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

public sealed class TableSourceItem : TableSource
{
    public Identifier Identifier { get; }
    public Identifier? Alias { get; }

    public TableSourceItem(Identifier identifier, Identifier? alias = null)
    {
        Identifier = identifier;
        Alias = alias;
    }
}

public sealed class TableSourceSubQuery : TableSource
{
    public IReadOnlyList<UnionStatement> Query { get; }
    public string Alias { get; }

    public TableSourceSubQuery(IReadOnlyList<UnionStatement> query, string alias)
    {
        Query = query;
        Alias = alias;
    }
}

public sealed class JoinStatement : ISqlNode
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
    Right,
}

public sealed class WhereClause : ISqlNode
{
    public Expression Expression { get; }

    public WhereClause(Expression expression)
    {
        Expression = expression;
    }
}

public sealed class GroupByClause : ISqlNode
{
    public IReadOnlyList<ColumnSource> Columns { get; }

    public GroupByClause(IReadOnlyList<ColumnSource> columns)
    {
        Columns = columns;
    }
}

public sealed class HavingClause : ISqlNode
{
    public Expression Expression { get; }

    public HavingClause(Expression expression)
    {
        Expression = expression;
    }
}

public sealed class OrderByClause : ISqlNode
{
    public IReadOnlyList<OrderByItem> Items { get; }

    public OrderByClause(IReadOnlyList<OrderByItem> items)
    {
        Items = items;
    }
}

public sealed class OrderByItem : ISqlNode
{
    public Identifier Identifier { get; }
    public FunctionArguments? Arguments { get; }
    public OrderDirection Direction { get; }

    public OrderByItem(Identifier identifier, FunctionArguments? arguments, OrderDirection direction)
    {
        Identifier = identifier;
        Arguments = arguments;
        Direction = direction;
    }
}

public enum OrderDirection
{
    NotSpecified,
    Asc,
    Desc,
}

public sealed class LimitClause : ISqlNode
{
    public Expression Expression { get; }

    public LimitClause(Expression expression)
    {
        Expression = expression;
    }
}

public sealed class OffsetClause : ISqlNode
{
    public Expression Expression { get; }

    public OffsetClause(Expression expression)
    {
        Expression = expression;
    }
}

public sealed class OverClause : ISqlNode
{
    public PartitionByClause? PartitionBy { get; }
    public OrderByClause? OrderBy { get; }

    public OverClause(PartitionByClause? partitionBy = null, OrderByClause? orderBy = null)
    {
        PartitionBy = partitionBy;
        OrderBy = orderBy;
    }
}

public sealed class PartitionByClause : ISqlNode
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

public sealed class BinaryExpression : Expression
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
    NotLike,
}

public sealed class UnaryExpression : Expression
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
    BitwiseNot,
}

public sealed class BetweenExpression : Expression
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

public sealed class InExpression : Expression
{
    public Expression Expression { get; }
    public bool IsNot { get; }
    public FunctionArguments Values { get; }

    public InExpression(Expression expression, FunctionArguments values, bool isNot = false)
    {
        Expression = expression;
        Values = values;
        IsNot = isNot;
    }
}

public sealed class IdentifierExpression : Expression
{
    public Identifier Identifier { get; }

    public IdentifierExpression(Identifier identifier)
    {
        Identifier = identifier;
    }
}

public sealed class LiteralExpression<T> : Expression
{
    public T Value { get; }

    public LiteralExpression(T value)
    {
        Value = value;
    }
}

public sealed class FunctionCall : Expression
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

public sealed class EmptyArguments : FunctionArguments
{
    public static readonly EmptyArguments Instance = new();

    private EmptyArguments()
    {
    }
}

public sealed class StarArgument : FunctionArguments
{
    public static readonly StarArgument Instance = new();

    private StarArgument()
    {
    }
}

public sealed class SelectStatementArgument : FunctionArguments
{
    public SelectStatement SelectStatement { get; }

    public SelectStatementArgument(SelectStatement selectStatement)
    {
        SelectStatement = selectStatement;
    }
}

public sealed class ExpressionListArguments : FunctionArguments
{
    public IReadOnlyList<Expression> Expressions { get; }

    public ExpressionListArguments(IReadOnlyList<Expression> expressions)
    {
        Expressions = expressions;
    }
}

public sealed class TupleExpression : Expression
{
    public IReadOnlyList<Expression> Expressions { get; }

    public TupleExpression(IReadOnlyList<Expression> expressions)
    {
        Expressions = expressions;
    }
}

public sealed class ParenthesizedSelectStatement : Expression
{
    public SelectStatement SelectStatement { get; }

    public ParenthesizedSelectStatement(SelectStatement selectStatement)
    {
        SelectStatement = selectStatement;
    }
}

public sealed class ParameterExpression : Expression
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
public sealed class Identifier : ISqlNode
{
    public static readonly Identifier STAR = new (["*"]);

    private string _cachedToString = null!;

    public IReadOnlyList<string> Parts { get; }

    public Identifier(IReadOnlyList<string> parts)
    {
        Parts = parts;
    }

    public override string ToString()
    {
        return _cachedToString ??= (Parts.Count == 1 ? Parts[0] : string.Join(".", Parts));
    }
}