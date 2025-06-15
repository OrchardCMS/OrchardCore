using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data;
using OrchardCore.Environment.Shell.Scope;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Queries.Core;

public static class QueriesDocumentMigrationHelper
{
    public static void Migrate(string source)
    {
        // To avoid concurrency issue when running this method from multiple features,
        // we must run this into a deferred task so each time it's called it runs in a new scope.
        ShellScope.AddDeferredTask(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IStore>();
            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();
            var queryManager = scope.ServiceProvider.GetRequiredService<IQueryManager>();

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
            await connection.OpenAsync().ConfigureAwait(false);
            var jsonContent = await connection.QueryFirstOrDefaultAsync<string>(sqlBuilder.ToSqlString()).ConfigureAwait(false);

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
                if (!string.Equals(queryObject.Value["Source"].GetValue<string>(), source, StringComparison.Ordinal))
                {
                    continue;
                }

                var query = await queryManager.NewAsync(source, queryObject.Value).ConfigureAwait(false);

                if (query == null)
                {
                    continue;
                }

                queries.Add(query);
            }

            await queryManager.SaveAsync(queries.ToArray()).ConfigureAwait(false);
        });
    }
}
