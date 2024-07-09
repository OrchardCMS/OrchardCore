using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Dapper;
using OrchardCore.Data;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Queries.Core;

public static class QueriesDocumentMigrationHelper
{
    public static async Task MigrateAsync(string source, IQueryManager queryManager, IStore store, IDbConnectionAccessor dbConnectionAccessor)
    {
        var documentTableName = store.Configuration.TableNameConvention.GetDocumentTable();
        var table = $"{store.Configuration.TablePrefix}{documentTableName}";
        var dialect = store.Configuration.SqlDialect;
        var quotedTableName = dialect.QuoteForTableName(table, store.Configuration.Schema);
        var quotedContentColumnName = dialect.QuoteForColumnName("Content");
        var quotedTypeColumnName = dialect.QuoteForColumnName("Type");

        var sqlBuilder = new SqlBuilder(store.Configuration.TablePrefix, store.Configuration.SqlDialect);
        sqlBuilder.AddSelector(quotedContentColumnName);
        sqlBuilder.From(quotedTableName);
        sqlBuilder.WhereAnd($" {quotedTypeColumnName} = 'OrchardCore.Queries.Services.QueriesDocument, OrchardCore.Queries' ");
        sqlBuilder.Take("1");

        await using var connection = dbConnectionAccessor.CreateConnection();
        await connection.OpenAsync();
        var jsonContent = await connection.QueryFirstOrDefaultAsync<string>(sqlBuilder.ToSqlString());

        if (string.IsNullOrEmpty(jsonContent))
        {
            return;
        }

        var jsonObject = JsonNode.Parse(jsonContent);

        if (jsonObject["Queries"] is not JsonObject queriesObject)
        {
            return;
        }

        var queries = new List<Query>();

        foreach (var queryObject in queriesObject)
        {
            if (queryObject.Value["Source"].GetValue<string>() != source)
            {
                continue;
            }

            var query = await queryManager.NewAsync(source, queryObject.Value);

            if (query == null)
            {
                continue;
            }

            queries.Add(query);
        }

        await queryManager.SaveAsync(queries.ToArray());
    }
}
