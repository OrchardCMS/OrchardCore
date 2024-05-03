using System;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Scope;
using YesSql;

namespace OrchardCore.Twitter.Migrations;

public class TwitterMigrations : DataMigration
{
    private readonly ShellDescriptor _shellDescriptor;

    public TwitterMigrations(ShellDescriptor shellDescriptor)
    {
        _shellDescriptor = shellDescriptor;
    }

    public int Create()
    {
        if (_shellDescriptor.WasFeatureAlreadyInstalled("OrchardCore.Twitter"))
        {
            Upgrade();
        }

        return 1;
    }

    private void Upgrade()
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var dbConnectionAccessor = scope.ServiceProvider.GetService<IDbConnectionAccessor>();
            var logger = scope.ServiceProvider.GetService<ILogger<TwitterMigrations>>();
            var tablePrefix = session.Store.Configuration.TablePrefix;
            var documentTableName = session.Store.Configuration.TableNameConvention.GetDocumentTable();
            var table = $"{session.Store.Configuration.TablePrefix}{documentTableName}";

            logger.LogDebug("Updating Twitter/X Workflow Items");

            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync(session.Store.Configuration.IsolationLevel);
            var dialect = session.Store.Configuration.SqlDialect;

            try
            {
                var quotedTableName = dialect.QuoteForTableName(table, session.Store.Configuration.Schema);
                var quotedContentColumnName = dialect.QuoteForColumnName("Content");
                var quotedTypeColumnName = dialect.QuoteForColumnName("Type");

                var updateCmd = $"UPDATE {quotedTableName} SET {quotedContentColumnName} = REPLACE({quotedContentColumnName}, 'UpdateTwitterStatusTask', 'UpdateXStatusTask') WHERE {quotedTypeColumnName} = 'OrchardCore.Workflows.Models.WorkflowType, OrchardCore.Workflows.Abstractions'";

                await transaction.Connection.ExecuteAsync(updateCmd, null, transaction);

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                logger.LogError(e, "An error occurred while updating Twitter/X Workflow Items");

                throw;
            }
        });
    }


}
