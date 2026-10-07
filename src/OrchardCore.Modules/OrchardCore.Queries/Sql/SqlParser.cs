using Cyqwel.Ast;
using Cyqwel.Dialects;
using Cyqwel.Generation;
using Cyqwel.Validation;
using Cyqwel.Visitors;
using YesSql;
using YesSql.Provider.MySql;
using YesSql.Provider.PostgreSql;
using YesSql.Provider.Sqlite;
using YesSql.Provider.SqlServer;

namespace OrchardCore.Queries.Sql;

public class SqlParser
{
    private const string RandomFunctionName = "__orchard_random__";

    private static readonly SqlGenerationOptions GenerationOptions = new()
    {
        FunctionNameCase = FunctionNameCase.Preserve,
    };

    private static readonly Cyqwel.Dialects.SqlDialect ParserDialect = SqlDialectBuilder
        .Create("orchard")
        .BasedOn(SqlDialects.Generic)
        .ConfigureParser(static options => options with { SupportsParameterDefaults = true })
        .Build();

    internal static IReadOnlyList<string> Validate(string sql)
    {
        var result = SqlValidator.Validate(
            sql,
            ParserDialect);

        if (!result.IsValid)
        {
            return result.Diagnostics
                .Where(static diagnostic => diagnostic.Severity == SqlValidationSeverity.Error)
                .Select(static diagnostic => diagnostic.Location is { } location
                    ? $"Parse error: {diagnostic.Message} at line {location.Line}, column {location.Column}"
                    : $"Parse error: {diagnostic.Message}")
                .ToArray();
        }

        if (!ParserDialect.TryParse(sql, out var document, out var error))
        {
            return error is null
                ? ["Parse error: Unknown parsing error"]
                : [$"Parse error: {error.Message} at line {error.Line}, column {error.Column}"];
        }

        return ContainsNonQueryStatement(document!)
            ? ["Only SELECT statements are supported."]
            : [];
    }

    public static bool TryParse(
        string sql,
        string schema,
        ISqlDialect dialect,
        string tablePrefix,
        IDictionary<string, object> parameters,
        out string query,
        out IEnumerable<string> messages)
    {
        if (!ParserDialect.TryParse(sql, out var document, out var error))
        {
            query = null;
            messages = error is null
                ? ["Parse error: Unknown parsing error"]
                : [$"Parse error: {error.Message} at position {error.Offset}"];
            return false;
        }

        if (ContainsNonQueryStatement(document!))
        {
            query = null;
            messages = ["Only SELECT statements are supported."];
            return false;
        }

        try
        {
            var names = new SqlNameCollector();
            names.Visit(document);

            var rewriter = new OrchardSqlRewriter(
                schema,
                tablePrefix,
                dialect,
                parameters,
                names.TableAliases,
                names.CommonTableExpressions);

            var rewritten = rewriter.Visit(document);
            query = new OrchardSqlDialect(dialect).Generate(rewritten, GenerationOptions) + ";";
            messages = [];
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or NotSupportedException)
        {
            query = null;
            messages = [exception.Message];
            return false;
        }
        catch (Exception exception)
        {
            query = null;
            messages = ["Unexpected error: " + exception.Message];
            return false;
        }
    }

    private static bool ContainsNonQueryStatement(SqlNode node) =>
        node.DescendantsAndSelf().OfType<SqlStatement>().Any(static statement => statement is not SqlQuery);

    private sealed class SqlNameCollector : SqlVisitor
    {
        public HashSet<string> TableAliases { get; } = [];

        public HashSet<string> CommonTableExpressions { get; } = [];

        protected override void VisitNamedTable(NamedTable node)
        {
            if (node.Alias is not null)
            {
                TableAliases.Add(node.Alias.Value);
            }

            base.VisitNamedTable(node);
        }

        protected override void VisitDerivedTable(DerivedTable node)
        {
            TableAliases.Add(node.Alias.Value);
            base.VisitDerivedTable(node);
        }

        protected override void VisitCommonTableExpression(CommonTableExpression node)
        {
            CommonTableExpressions.Add(node.Name.Value);
            base.VisitCommonTableExpression(node);
        }
    }

    private sealed class OrchardSqlRewriter(
        string schema,
        string tablePrefix,
        ISqlDialect dialect,
        IDictionary<string, object> parameters,
        HashSet<string> tableAliases,
        HashSet<string> commonTableExpressions) : SqlRewriter
    {
        protected override SqlNode VisitSelect(SelectStatement node)
        {
            var rewritten = (SelectStatement)base.VisitSelect(node);
            var projections = rewritten.Projections;

            for (var i = 0; i < projections.Count; i++)
            {
                if (projections[i].Expression is not LiteralExpression { Value: bool value })
                {
                    continue;
                }

                if (ReferenceEquals(projections, rewritten.Projections))
                {
                    projections = rewritten.Projections.ToArray();
                }

                ((SelectItem[])projections)[i] = projections[i] with
                {
                    Expression = new ColumnExpression([Quote(value.ToString().ToLowerInvariant())]),
                };
            }

            var limit = GetLimit(rewritten.Limit, rewritten.Offset);

            return ReferenceEquals(projections, rewritten.Projections) && ReferenceEquals(limit, rewritten.Limit)
                ? rewritten
                : rewritten with { Projections = projections, Limit = limit };
        }

        protected override SqlNode VisitSetOperation(SetOperationStatement node)
        {
            var rewritten = (SetOperationStatement)base.VisitSetOperation(node);
            var limit = GetLimit(rewritten.Limit, rewritten.Offset);

            return ReferenceEquals(limit, rewritten.Limit)
                ? rewritten
                : rewritten with { Limit = limit };
        }

        protected override SqlNode VisitTableName(TableName node)
        {
            if (node.Parts.Count == 0)
            {
                return node;
            }

            var parts = new List<SqlIdentifier>(node.Parts.Count + 1);
            var first = node.Parts[0];

            if (commonTableExpressions.Contains(first.Value))
            {
                parts.Add(Quote(first.Value));
            }
            else
            {
                parts.AddRange(GetTableNameParts(tablePrefix + first.Value));
            }

            for (var i = 1; i < node.Parts.Count; i++)
            {
                parts.Add(Quote(node.Parts[i].Value));
            }

            return node with { Parts = parts };
        }

        protected override SqlNode VisitColumn(ColumnExpression node)
        {
            if (node.Parts.Count == 0)
            {
                return node;
            }

            if (node.Parts.Count == 1)
            {
                return node with { Parts = [Quote(node.Parts[0].Value)] };
            }

            var parts = new List<SqlIdentifier>(node.Parts.Count + 1);
            var qualifier = node.Parts[0];

            if (tableAliases.Contains(qualifier.Value))
            {
                parts.Add(qualifier with { IsQuoted = false });
            }
            else
            {
                parts.AddRange(GetTableNameParts(tablePrefix + qualifier.Value));
            }

            for (var i = 1; i < node.Parts.Count; i++)
            {
                parts.Add(Quote(node.Parts[i].Value));
            }

            return node with { Parts = parts };
        }

        protected override SqlNode VisitParameter(ParameterExpression node)
        {
            if (parameters is not null && !parameters.ContainsKey(node.Name))
            {
                parameters[node.Name] = node.DefaultValue switch
                {
                    LiteralExpression { Value: bool or string or decimal or long } literal => literal.Value,
                    null => throw new InvalidOperationException($"Missing parameter: {node.Name}"),
                    _ => throw new InvalidOperationException("Unsupported default parameter value type"),
                };
            }

            return node;
        }

        protected override SqlNode VisitOrderByItem(OrderByItem node)
        {
            var rewritten = (OrderByItem)base.VisitOrderByItem(node);

            return rewritten.Expression is FunctionCallExpression
                {
                    Name.Value: var name,
                    Arguments.Count: 0,
                } function
                && name.Equals("random", StringComparison.OrdinalIgnoreCase)
                    ? rewritten with
                    {
                        Expression = function with { Name = new SqlIdentifier(RandomFunctionName) },
                    }
                    : rewritten;
        }

        private IReadOnlyList<SqlIdentifier> GetTableNameParts(string tableName)
        {
            var quotedTableName = dialect.QuoteForTableName(tableName, schema);

            return !string.IsNullOrEmpty(schema) && quotedTableName.Contains('.', StringComparison.Ordinal)
                ? [Quote(schema), Quote(tableName)]
                : [Quote(tableName)];
        }

        private SqlExpression GetLimit(SqlExpression limit, SqlExpression offset)
        {
            if (offset is null || limit is not null)
            {
                return limit;
            }

            return dialect switch
            {
                SqliteDialect => new LiteralExpression(-1L),
                PostgreSqlDialect => new ColumnExpression([new SqlIdentifier("all")]),
                MySqlDialect => new LiteralExpression(18446744073709551610M),
                _ => null,
            };
        }

        private static SqlIdentifier Quote(string value) => new(value, IsQuoted: true);
    }

    private sealed class OrchardSqlDialect(ISqlDialect dialect)
        : Cyqwel.Dialects.SqlDialect(
            dialect.Name,
            GetOpenQuote(dialect),
            GetCloseQuote(dialect),
            GetLimitStyle(dialect))
    {
        public override string RenderLiteral(LiteralExpression literal, SqlGenerationOptions options) =>
            dialect.GetSqlValue(literal.Value);

        public override string RenderFunction(
            FunctionCallExpression function,
            Func<SqlExpression, string> renderExpression,
            SqlGenerationOptions options)
        {
            if (function.Name.Value == RandomFunctionName)
            {
                return dialect.RandomOrderByClause;
            }

            var arguments = function.Arguments.Select(renderExpression).ToArray();
            if (function.IsDistinct && arguments.Length > 0)
            {
                arguments[0] = "DISTINCT " + arguments[0];
            }

            return dialect.RenderMethod(function.Name.Value, arguments);
        }

        public override bool ShouldQuoteIdentifier(SqlIdentifier identifier) => identifier.IsQuoted;

        private static char GetOpenQuote(ISqlDialect dialect) => GetQuotedIdentifier(dialect)[0];

        private static char GetCloseQuote(ISqlDialect dialect) => GetQuotedIdentifier(dialect)[^1];

        private static string GetQuotedIdentifier(ISqlDialect dialect) =>
            dialect.QuoteForColumnName("identifier");

        private static SqlLimitStyle GetLimitStyle(ISqlDialect dialect) =>
            dialect is SqlServerDialect ? SqlLimitStyle.Top : SqlLimitStyle.LimitOffset;
    }
}
