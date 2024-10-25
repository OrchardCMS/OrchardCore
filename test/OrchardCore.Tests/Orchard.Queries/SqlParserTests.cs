using OrchardCore.Queries.Sql;

namespace OrchardCore.Tests.OrchardCore.Queries;

public class SqlParserTests
{
    private readonly ISqlDialect _defaultDialect = new SqlServerDialect();
    private readonly string _schema = new Configuration().Schema;
    private readonly string _defaultTablePrefix = "tp_";

    private static string FormatSql(string sql)
    {
        return sql.Replace("\r\n", " ").Replace('\n', ' ');
    }

    [Theory]
    [InlineData("select a", "SELECT [a];")]
    [InlineData("select *", "SELECT *;")]
    [InlineData("SELECT a", "SELECT [a];")]
    [InlineData("SELECT a, b", "SELECT [a], [b];")]
    [InlineData("SELECT a.b", "SELECT [tp_a].[b];")]
    [InlineData("SELECT a.b, c.d", "SELECT [tp_a].[b], [tp_c].[d];")]
    [InlineData("SELECT a as a1", "SELECT [a] AS a1;")]
    [InlineData("SELECT a as a1, b as b1", "SELECT [a] AS a1, [b] AS b1;")]
    [InlineData("select Avg(a)", "SELECT Avg([a]);")]
    [InlineData("select Min(*)", "SELECT Min(*);")]
    [InlineData("select distinct a", "SELECT DISTINCT [a];")]
    public void ShouldParseSelectClause(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Theory]
    [InlineData("select a from t", "SELECT [a] FROM [tp_t];")]
    [InlineData("SELECT a FROM t", "SELECT [a] FROM [tp_t];")]
    [InlineData("SELECT a FROM t as t1", "SELECT [a] FROM [tp_t] AS t1;")]
    [InlineData("SELECT a FROM t1, t2", "SELECT [a] FROM [tp_t1], [tp_t2];")]
    public void ShouldParseFromClause(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Theory]
    [InlineData("select a where b", "SELECT [a] WHERE [b];")]
    [InlineData("select a where b.b1", "SELECT [a] WHERE [tp_b].[b1];")]
    [InlineData("select a where b = c", "SELECT [a] WHERE [b] = [c];")]
    [InlineData("select a where b = c and d", "SELECT [a] WHERE [b] = [c] AND [d];")]
    public void ShouldParseWhereClause(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Theory]
    [InlineData("select a where a", "SELECT [a] WHERE [a];")]
    [InlineData("select a where ~a", "SELECT [a] WHERE ~[a];")]
    [InlineData("select a where a = b", "SELECT [a] WHERE [a] = [b];")]
    [InlineData("select a where a = true", "SELECT [a] WHERE [a] = 1;")]
    [InlineData("select a where a = false", "SELECT [a] WHERE [a] = 0;")]
    [InlineData("select a where a = 1", "SELECT [a] WHERE [a] = 1;")]
    [InlineData("select a where a = 1.234", "SELECT [a] WHERE [a] = 1.234;")]
    [InlineData("select a where a = 'foo'", "SELECT [a] WHERE [a] = N'foo';")]
    [InlineData("select a where a like '%foo%'", "SELECT [a] WHERE [a] LIKE N'%foo%';")]
    [InlineData("select a where a not like '%foo%'", "SELECT [a] WHERE [a] NOT LIKE N'%foo%';")]
    [InlineData("select a where a between b and c", "SELECT [a] WHERE [a] BETWEEN [b] AND [c];")]
    [InlineData("select a where a not between b and c", "SELECT [a] WHERE [a] NOT BETWEEN [b] AND [c];")]
    [InlineData("select a where a = b or c = d", "SELECT [a] WHERE [a] = [b] OR [c] = [d];")]
    [InlineData("select a where (a = b) or (c = d)", "SELECT [a] WHERE ([a] = [b]) OR ([c] = [d]);")]
    [InlineData("select a where (a = b) or (c = d) and e", "SELECT [a] WHERE ([a] = [b]) OR ([c] = [d]) AND [e];")]
    [InlineData("select a where test(arg)", "SELECT [a] WHERE test([arg]);")]
    [InlineData("select a where b in (1,2,3)", "SELECT [a] WHERE [b] IN (1, 2, 3);")]
    [InlineData("select a where b in (select b)", "SELECT [a] WHERE [b] IN (SELECT [b]);")]
    [InlineData("select a where b not in (1,2,3)", "SELECT [a] WHERE [b] NOT IN (1, 2, 3);")]
    [InlineData("select a where b not in (select b)", "SELECT [a] WHERE [b] NOT IN (SELECT [b]);")]
    [InlineData("select a where b = (select Avg(c) from d)", "SELECT [a] WHERE [b] = (SELECT Avg([c]) FROM [tp_d]);")]
    public void ShouldParseExpression(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Theory]
    [InlineData("select a where a = @b", "SELECT [a] WHERE [a] = @b;")]
    [InlineData("select a where a = @b limit @limit", "SELECT TOP (@limit) [a] WHERE [a] = @b;")]
    [InlineData("select a where a = @b limit @limit:10", "SELECT TOP (@limit) [a] WHERE [a] = @b;")]
    public void ShouldParseParameters(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Fact]
    public void ShouldDefineDefaultParametersValue()
    {
        var parameters = new Dictionary<string, object>();
        var result = SqlParser.TryParse("select a where a = @b:10", _schema, _defaultDialect, _defaultTablePrefix, parameters, out _, out _);
        Assert.True(result);
        Assert.Equal(10, parameters["b"]);
    }

    [Theory]
    [InlineData("select a from b inner join c on b.b1 = c.c1", "SELECT [a] FROM [tp_b] INNER JOIN [tp_c] ON [tp_b].[b1] = [tp_c].[c1];")]
    [InlineData("select a from b as ba inner join c as ca on ba.b1 = ca.c1", "SELECT [a] FROM [tp_b] AS ba INNER JOIN [tp_c] AS ca ON ba.[b1] = ca.[c1];")]
    [InlineData("select a from b inner join c on b.b1 = c.c1 left join d on d.a = d.b", "SELECT [a] FROM [tp_b] INNER JOIN [tp_c] ON [tp_b].[b1] = [tp_c].[c1] LEFT JOIN [tp_d] ON [tp_d].[a] = [tp_d].[b];")]
    [InlineData("select a from b inner join c on b.b1 = c.c1 and b.b2 = c.c2", "SELECT [a] FROM [tp_b] INNER JOIN [tp_c] ON [tp_b].[b1] = [tp_c].[c1] AND [tp_b].[b2] = [tp_c].[c2];")]
    [InlineData("select a from b inner join c on b.b1 = c.c1 and b.b2 = @param", "SELECT [a] FROM [tp_b] INNER JOIN [tp_c] ON [tp_b].[b1] = [tp_c].[c1] AND [tp_b].[b2] = @param;")]
    [InlineData("select a from b inner join c on 1 = 1 and @param = 'foo'", "SELECT [a] FROM [tp_b] INNER JOIN [tp_c] ON 1 = 1 AND @param = N'foo';")]
    [InlineData("select a from b inner join c on 1 = @param left join d on d.a = @param left join e on e.a = 'foo'", "SELECT [a] FROM [tp_b] INNER JOIN [tp_c] ON 1 = @param LEFT JOIN [tp_d] ON [tp_d].[a] = @param LEFT JOIN [tp_e] ON [tp_e].[a] = N'foo';")]
    public void ShouldParseJoinClause(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Theory]
    [InlineData("select a order by b", "SELECT [a] ORDER BY [b];")]
    [InlineData("select a order by b, c", "SELECT [a] ORDER BY [b], [c];")]
    [InlineData("select a order by b.c", "SELECT [a] ORDER BY [tp_b].[c];")]
    [InlineData("select a from b as b1 order by b1.c", "SELECT [a] FROM [tp_b] AS b1 ORDER BY b1.[c];")]
    [InlineData("select a order by b asc", "SELECT [a] ORDER BY [b] ASC;")]
    [InlineData("select a order by b desc", "SELECT [a] ORDER BY [b] DESC;")]
    public void ShouldParseOrderByClause(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Theory]
    [InlineData("select a limit 100", "SELECT TOP (100) [a];")]
    [InlineData("select a limit 100 offset 10", "SELECT [a] OFFSET 10 ROWS FETCH NEXT 100 ROWS ONLY;")]
    [InlineData("select a offset 10", "SELECT [a] OFFSET 10 ROWS;")]
    public void ShouldParseLimitOffsetClause(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Theory]
    [InlineData("select count(a) group by b", "SELECT count([a]) GROUP BY [b];")]
    [InlineData("select count(a) group by b, c", "SELECT count([a]) GROUP BY [b], [c];")]
    [InlineData("select count(a) group by b.c", "SELECT count([a]) GROUP BY [tp_b].[c];")]
    [InlineData("select count(a) from b as b1 group by b1.c", "SELECT count([a]) FROM [tp_b] AS b1 GROUP BY b1.[c];")]
    [InlineData("select Month(a) as m group by Month(a)", "SELECT Month([a]) AS m GROUP BY Month([a]);")]
    public void ShouldParseGroupByClause(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Theory]
    [InlineData("SELECT COUNT(CustomerID) GROUP BY Country HAVING COUNT(CustomerID) > 5", "SELECT COUNT([CustomerID]) GROUP BY [Country] HAVING COUNT([CustomerID]) > 5;")]
    public void ShouldParseHavingClause(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Fact]
    public void ShouldReturnErrorMessage()
    {
        var result = SqlParser.TryParse("SEL a", _schema, _defaultDialect, _defaultTablePrefix, null, out _, out var messages);

        Assert.False(result);
        Assert.Single(messages);
        Assert.Contains("at line:0, col:0", messages.First());
    }

    [Theory]
    [InlineData("SELECT a; -- this is a comment", "SELECT [a];")]
    [InlineData("SELECT a; \n-- this is a comment", "SELECT [a];")]
    [InlineData("SELECT /* comment */ a;", "SELECT [a];")]
    [InlineData("SELECT /* comment \n comment */ a;", "SELECT [a];")]
    public void ShouldParseComments(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Theory]
    [InlineData("select COUNT(1) over () from a", "SELECT COUNT(1) OVER () FROM [tp_a];")]
    [InlineData("select COUNT(1) over () a from b", "SELECT COUNT(1) OVER () AS a FROM [tp_b];")]
    [InlineData("select COUNT(1) over (partition by a) from b", "SELECT COUNT(1) OVER (PARTITION BY [a]) FROM [tp_b];")]
    [InlineData("select COUNT(1) over (order by a) from b", "SELECT COUNT(1) OVER (ORDER BY [a]) FROM [tp_b];")]
    [InlineData("select COUNT(1) over (order by a, b) from c", "SELECT COUNT(1) OVER (ORDER BY [a], [b]) FROM [tp_c];")]
    [InlineData("select COUNT(1) over (order by a asc, b desc, c desc) from d", "SELECT COUNT(1) OVER (ORDER BY [a] ASC, [b] DESC, [c] DESC) FROM [tp_d];")]
    [InlineData("select COUNT(1) over (partition by a order by b) from c", "SELECT COUNT(1) OVER (PARTITION BY [a] ORDER BY [b]) FROM [tp_c];")]
    [InlineData("select COUNT(1) over () a, MAX(b) over () c from d", "SELECT COUNT(1) OVER () AS a, MAX([b]) OVER () AS c FROM [tp_d];")]
    public void ShouldParseWindowFunction(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Theory]
    [InlineData("select a from b union select c from d", "SELECT [a] FROM [tp_b] UNION SELECT [c] FROM [tp_d];")]
    [InlineData("select a from b union all select c from d", "SELECT [a] FROM [tp_b] UNION ALL SELECT [c] FROM [tp_d];")]
    [InlineData("select a from b union all select c from d union select e from f", "SELECT [a] FROM [tp_b] UNION ALL SELECT [c] FROM [tp_d] UNION SELECT [e] FROM [tp_f];")]
    public void ShouldParseUnionClause(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Theory]
    [InlineData("with cte as (select a from t) select * from cte", "WITH cte AS (SELECT [a] FROM [tp_t]) SELECT * FROM [cte];")]
    [InlineData("with cte as (select a from t), cte2 as (select b from t) select * from cte2", "WITH cte AS (SELECT [a] FROM [tp_t]), cte2 AS (SELECT [b] FROM [tp_t]) SELECT * FROM [cte2];")]
    [InlineData("with cte as (select a from t union all select b from t) select * from cte", "WITH cte AS (SELECT [a] FROM [tp_t] UNION ALL SELECT [b] FROM [tp_t]) SELECT * FROM [cte];")]
    [InlineData("with cte1(abc, def) as (select id as abc, id as def from t), cte2(ghi,jkl) as (select abc as ghi, def as jkl from cte1) select ghi, jkl from cte2", "WITH cte1(abc, def) AS (SELECT [id] AS abc, [id] AS def FROM [tp_t]), cte2(ghi, jkl) AS (SELECT [abc] AS ghi, [def] AS jkl FROM [cte1]) SELECT [ghi], [jkl] FROM [cte2];")]
    [InlineData("with test(test) as (select a as test from t) select test from test", "WITH test(test) AS (SELECT [a] AS test FROM [tp_t]) SELECT [test] FROM [test];")]
    public void ShouldParseCte(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }

    [Theory]
    [InlineData("select * from (select * from t) as b", "SELECT * FROM (SELECT * FROM [tp_t]) AS b;")]
    [InlineData("select b.a from (select a from t) as b where b.a = b.a", "SELECT b.[a] FROM (SELECT [a] FROM [tp_t]) AS b WHERE b.[a] = b.[a];")]
    [InlineData("select c.b from (select a as b from t) as c", "SELECT c.[b] FROM (SELECT [a] AS b FROM [tp_t]) AS c;")]
    [InlineData("select b.a from (select a from t where [a] = 1) as b where b.a = 1", "SELECT b.[a] FROM (SELECT [a] FROM [tp_t] WHERE [a] = 1) AS b WHERE b.[a] = 1;")]
    [InlineData("select b.a from (select a from t where [a] = 1 union all select a from t where [a] = 2) as b", "SELECT b.[a] FROM (SELECT [a] FROM [tp_t] WHERE [a] = 1 UNION ALL SELECT [a] FROM [tp_t] WHERE [a] = 2) AS b;")]
    [InlineData("select d.b, g.b from (select a as b from c) as d, (select e as b from f) as g", "SELECT d.[b], g.[b] FROM (SELECT [a] AS b FROM [tp_c]) AS d, (SELECT [e] AS b FROM [tp_f]) AS g;")]
    [InlineData("select * from (select d as e from (select c as d from (select b as c from (select a as b from t) as l4) as l3) as l2) as l1", "SELECT * FROM (SELECT [d] AS e FROM (SELECT [c] AS d FROM (SELECT [b] AS c FROM (SELECT [a] AS b FROM [tp_t]) AS l4) AS l3) AS l2) AS l1;")]
    public void ShouldParseSubquery(string sql, string expectedSql)
    {
        var result = SqlParser.TryParse(sql, _schema, _defaultDialect, _defaultTablePrefix, null, out var rawQuery, out _);
        Assert.True(result);
        Assert.Equal(expectedSql, FormatSql(rawQuery));
    }
}
