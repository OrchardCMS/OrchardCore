using System.Linq;
using Orchard.Queries.Sql;
using Xunit;
using YesSql;
using YesSql.Provider.SqlServer;

namespace Orchard.Tests.Orchard.Queries
{
    public class SqlParserTests
    {
        private ISqlDialect _defaultDialect = new SqlServerDialect();
        private string _defaultTablePrefix = "tp_";

        private string FormatSql(string sql)
        {
            return sql.Replace("\r\n", " ").Replace("\n", " ");
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
            var result = SqlParser.TryParse(sql, _defaultDialect, _defaultTablePrefix, out var rawQuery, out var rawParameters, out var messages);
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
            var result = SqlParser.TryParse(sql, _defaultDialect, _defaultTablePrefix, out var rawQuery, out var rawParameters, out var messages);
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
            var result = SqlParser.TryParse(sql, _defaultDialect, _defaultTablePrefix, out var rawQuery, out var rawParameters, out var messages);
            Assert.True(result);
            Assert.Equal(expectedSql, FormatSql(rawQuery));
        }

        [Theory]
        [InlineData("select a where a", "SELECT [a] WHERE [a];", new object[0])]
        [InlineData("select a where ~a", "SELECT [a] WHERE ~[a];", new object[0])]
        [InlineData("select a where a = b", "SELECT [a] WHERE [a] = [b];", new object[0])]
        [InlineData("select a where a = true", "SELECT [a] WHERE [a] = @p0;", new object[] { true })]
        [InlineData("select a where a = false", "SELECT [a] WHERE [a] = @p0;", new object[] { false })]
        [InlineData("select a where a = 1", "SELECT [a] WHERE [a] = @p0;", new object[] { 1 })]
        [InlineData("select a where a = 1.234", "SELECT [a] WHERE [a] = @p0;", new object[] { 1.234 })]
        [InlineData("select a where a = 'foo'", "SELECT [a] WHERE [a] = @p0;", new object[] { "foo" })]
        [InlineData("select a where a between b and c", "SELECT [a] WHERE [a] BETWEEN [b] AND [c];", new object[0])]
        [InlineData("select a where a not between b and c", "SELECT [a] WHERE [a] NOT BETWEEN [b] AND [c];", new object[0])]
        [InlineData("select a where a = b or c = d", "SELECT [a] WHERE [a] = [b] OR [c] = [d];", new object[0])]
        [InlineData("select a where (a = b) or (c = d)", "SELECT [a] WHERE ([a] = [b]) OR ([c] = [d]);", new object[0])]
        [InlineData("select a where (a = b) or (c = d) and e", "SELECT [a] WHERE ([a] = [b]) OR ([c] = [d]) AND [e];", new object[0])]
        [InlineData("select a where test(arg)", "SELECT [a] WHERE test([arg]);", new object[0])]
        [InlineData("select a where b in (1,2,3)", "SELECT [a] WHERE [b] IN (@p0, @p1, @p2);", new object[] { 1, 2, 3 })]
        [InlineData("select a where b = (select Avg(c) from d)", "SELECT [a] WHERE [b] = (SELECT Avg([c]) FROM [tp_d]);", new object[0])]
        public void ShouldParseExpression(string sql, string expectedSql, object[] expectedParameters = null)
        {
            var result = SqlParser.TryParse(sql, _defaultDialect, _defaultTablePrefix, out var rawQuery, out var rawParameters, out var messages);
            Assert.True(result);
            Assert.Equal(expectedSql, FormatSql(rawQuery));

            if (expectedParameters != null)
            {
                for(var i=0; i<expectedParameters.Length; i++)
                {
                    Assert.Equal(expectedParameters[i], rawParameters["@p" + i]);
                }
            }
        }

        [Theory]
        [InlineData("select a from b inner join c on b.b1 = c.c1", "SELECT [a] FROM [tp_b] INNER JOIN [tp_c] ON [tp_b].[b1] = [tp_c].[c1];", new object[0])]
        [InlineData("select a from b as ba inner join c as ca on ba.b1 = ca.c1", "SELECT [a] FROM [tp_b] AS ba INNER JOIN [tp_c] AS ca ON ba.[b1] = ca.[c1];", new object[0])]
        public void ShouldParseJoinClause(string sql, string expectedSql, object[] expectedParameters = null)
        {
            var result = SqlParser.TryParse(sql, _defaultDialect, _defaultTablePrefix, out var rawQuery, out var rawParameters, out var messages);
            Assert.True(result);
            Assert.Equal(expectedSql, FormatSql(rawQuery));
        }


        [Theory]
        [InlineData("select a order by b", "SELECT [a] ORDER BY [b];", new object[0])]
        [InlineData("select a order by b, c", "SELECT [a] ORDER BY [b], [c];", new object[0])]
        [InlineData("select a order by b.c", "SELECT [a] ORDER BY [tp_b].[c];", new object[0])]
        [InlineData("select a from b as b1 order by b1.c", "SELECT [a] FROM [tp_b] AS b1 ORDER BY b1.[c];", new object[0])]
        [InlineData("select a order by b asc", "SELECT [a] ORDER BY [b] ASC;", new object[0])]
        [InlineData("select a order by b desc", "SELECT [a] ORDER BY [b] DESC;", new object[0])]
        public void ShouldParseOrderByClause(string sql, string expectedSql, object[] expectedParameters = null)
        {
            var result = SqlParser.TryParse(sql, _defaultDialect, _defaultTablePrefix, out var rawQuery, out var rawParameters, out var messages);
            Assert.True(result);
            Assert.Equal(expectedSql, FormatSql(rawQuery));
        }

        [Theory]
        [InlineData("select count(a) group by b", "SELECT count([a]) GROUP BY [b];", new object[0])]
        [InlineData("select count(a) group by b, c", "SELECT count([a]) GROUP BY [b], [c];", new object[0])]
        [InlineData("select count(a) group by b.c", "SELECT count([a]) GROUP BY [tp_b].[c];", new object[0])]
        [InlineData("select count(a) from b as b1 group by b1.c", "SELECT count([a]) FROM [tp_b] AS b1 GROUP BY b1.[c];", new object[0])]
        [InlineData("select Month(a) as m group by Month(a)", "SELECT Month([a]) AS m GROUP BY Month([a]);", new object[0])]
        public void ShouldParseGroupByClause(string sql, string expectedSql, object[] expectedParameters = null)
        {
            var result = SqlParser.TryParse(sql, _defaultDialect, _defaultTablePrefix, out var rawQuery, out var rawParameters, out var messages);
            Assert.True(result);
            Assert.Equal(expectedSql, FormatSql(rawQuery));
        }

        [Theory]
        [InlineData("SELECT COUNT(CustomerID) GROUP BY Country HAVING COUNT(CustomerID) > 5", "SELECT COUNT([CustomerID]) GROUP BY [Country] HAVING COUNT([CustomerID]) > @p0;", new object[0])]
        public void ShouldParseHavingClause(string sql, string expectedSql, object[] expectedParameters = null)
        {
            var result = SqlParser.TryParse(sql, _defaultDialect, _defaultTablePrefix, out var rawQuery, out var rawParameters, out var messages);
            Assert.True(result);
            Assert.Equal(expectedSql, FormatSql(rawQuery));
        }

        [Fact]
        public void ShouldReturnErrorMessage()
        {
            var result = SqlParser.TryParse("SEL a", _defaultDialect, _defaultTablePrefix, out var rawQuery, out var rawParameters, out var messages);

            Assert.False(result);
            Assert.Equal(1, messages.Count());
            Assert.Contains("at line:0, col:0", messages.First());
        }

        [Theory]
        [InlineData("SELECT a; -- this is a comment", "SELECT [a];", new object[0])]
        [InlineData("SELECT a; \n-- this is a comment", "SELECT [a];", new object[0])]
        [InlineData("SELECT /* comment */ a;", "SELECT [a];", new object[0])]
        [InlineData("SELECT /* comment \n comment */ a;", "SELECT [a];", new object[0])]
        public void ShouldParseComments(string sql, string expectedSql, object[] expectedParameters = null)
        {
            var result = SqlParser.TryParse(sql, _defaultDialect, _defaultTablePrefix, out var rawQuery, out var rawParameters, out var messages);
            Assert.True(result);
            Assert.Equal(expectedSql, FormatSql(rawQuery));
        }
    }
}
