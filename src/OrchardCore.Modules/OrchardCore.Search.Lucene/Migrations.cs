using System;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using YesSql;

namespace OrchardCore.Search.Lucene
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            return 1;
        }

        public int UpdateFrom1()
        {
            // Defer this until after the subsequent migrations have succeded as the schema has changed.
            ShellScope.AddDeferredTask(async scope =>
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var dbConnectionAccessor = scope.ServiceProvider.GetService<IDbConnectionAccessor>();
                var shellSettings = scope.ServiceProvider.GetService<ShellSettings>();
                var logger = scope.ServiceProvider.GetService<ILogger<Migrations>>();
                var tablePrefix = shellSettings["TablePrefix"];

                if (!String.IsNullOrEmpty(tablePrefix))
                {
                    tablePrefix += '_';
                }

                var table = $"{tablePrefix}{nameof(Document)}";

                using (var connection = dbConnectionAccessor.CreateConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction(session.Store.Configuration.IsolationLevel))
                    {
                        var dialect = session.Store.Configuration.SqlDialect;

                        try
                        {
                            if (logger.IsEnabled(LogLevel.Debug))
                            {
                                logger.LogDebug("Updating Lucene indices settings and queries");
                            }

                            var updateCmd = $"UPDATE {dialect.QuoteForTableName(table)} SET Content = REPLACE(content, '\"$type\":\"OrchardCore.Lucene.LuceneQuery, OrchardCore.Lucene\"', '\"$type\":\"OrchardCore.Search.Lucene.LuceneQuery, OrchardCore.Search.Lucene\"') WHERE [Type] = 'OrchardCore.Queries.Services.QueriesDocument, OrchardCore.Queries'";

                            await transaction.Connection.ExecuteAsync(updateCmd, null, transaction);

                            updateCmd = $"UPDATE {dialect.QuoteForTableName(table)} SET [Type] = 'OrchardCore.Search.Lucene.Model.LuceneIndexSettingsDocument, OrchardCore.Search.Lucene' WHERE [Type] = 'OrchardCore.Lucene.Model.LuceneIndexSettingsDocument, OrchardCore.Lucene'";

                            await transaction.Connection.ExecuteAsync(updateCmd, null, transaction);

                            transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            logger.LogError(e, "An error occurred while updating Lucene indices settings and queries");

                            throw;
                        }
                    }
                }
            });

            return 2;
        }
    }
}
