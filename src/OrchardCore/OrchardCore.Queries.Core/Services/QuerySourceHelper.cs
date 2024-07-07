using System.Collections.Generic;
using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data;
using OrchardCore.Environment.Shell.Scope;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Queries.Core.Services;

public static class QuerySourceHelper
{
    public static Query CreateQuery(string source, bool canReturnContentItems = true, JsonNode data = null)
    {
        var query = new Query()
        {
            Source = source,
            CanReturnContentItems = canReturnContentItems,
        };

        if (data != null)
        {
            var name = data[nameof(Query.Name)];

            if (name != null)
            {
                query.Name = name.GetValue<string>();
            }
            var schema = data[nameof(Query.Schema)];

            if (schema != null)
            {
                query.Schema = schema.GetValue<string>();
            }

            // For backward compatibility, we use the key 'ReturnDocuments'.
            var returnDocuments = data["ReturnDocuments"];

            if (returnDocuments != null)
            {
                query.ReturnContentItems = returnDocuments.GetValue<bool>();
            }

            var returnContentItems = data[nameof(Query.ReturnContentItems)];

            if (returnContentItems != null)
            {
                query.ReturnContentItems = returnContentItems.GetValue<bool>();
            }
        }

        return query;
    }

    public static void MigrateQueries(string source)
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

            var querySource = scope.ServiceProvider.GetRequiredKeyedService<IQuerySource>(source);

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
    }
}
