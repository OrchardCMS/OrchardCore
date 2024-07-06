using System.Collections.Generic;
using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Queries.Core;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Queries.Sql.Migrations;

public class SqlQueryMigrations : DataMigration
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
    public int Create()
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var dbConnectionAccessor = scope.ServiceProvider.GetService<IDbConnectionAccessor>();

            var documentTableName = session.Store.Configuration.TableNameConvention.GetDocumentTable();
            var table = $"{session.Store.Configuration.TablePrefix}{documentTableName}";
            var dialect = session.Store.Configuration.SqlDialect;
            var quotedTableName = dialect.QuoteForTableName(table, session.Store.Configuration.Schema);
            var quotedContentColumnName = dialect.QuoteForColumnName("Content");
            var quotedTypeColumnName = dialect.QuoteForColumnName("Type");

            var sqlBuilder = new SqlBuilder(session.Store.Configuration.TablePrefix, session.Store.Configuration.SqlDialect);
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

            var querySource = scope.ServiceProvider.GetRequiredKeyedService<IQuerySource>(SqlQuerySource.SourceName);

            var queries = new List<Query>();

            foreach (var queryObject in queriesObject)
            {
                if (queryObject.Value["Source"].GetValue<string>() != querySource.Name)
                {
                    continue;
                }

                var query = querySource.Create(queryObject.Value);

                queries.Add(query);
            }

            var queryManager = scope.ServiceProvider.GetRequiredService<IQueryManager>();

            await queryManager.SaveQueryAsync(queries.ToArray());
        });

        return 1;
    }
}
