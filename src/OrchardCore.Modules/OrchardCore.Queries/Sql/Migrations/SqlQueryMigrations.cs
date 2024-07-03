using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Queries.Sql.Models;
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

            foreach (var queryObject in queriesObject)
            {
                if (queryObject.Value["Source"].GetValue<string>() != SqlQuerySource.SourceName)
                {
                    continue;
                }

                var query = new Query
                {
                    Source = SqlQuerySource.SourceName,
                    Name = queryObject.Key,
                    Schema = queryObject.Value["Schema"].GetValue<string>(),
                    ReturnContentItems = queryObject.Value["ReturnDocuments"].GetValue<bool>()
                };

                query.Put(new SqlQueryMetadata
                {
                    Template = queryObject.Value["Template"].GetValue<string>(),
                });

                await session.SaveAsync(query);
            }

            await session.SaveChangesAsync();
        });

        return 1;
    }
}
