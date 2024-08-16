using System;
using System.Threading.Tasks;
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

public sealed class TwitterToXMigrations : DataMigration
{
    private readonly IDbConnectionAccessor _dbAccessor;
    private readonly IStore _store;
    private readonly ILogger<TwitterToXMigrations> _logger;
    private readonly string _tablePrefix;

    public TwitterToXMigrations(IDbConnectionAccessor dbAccessor, IStore store, ILogger<TwitterToXMigrations> logger, ShellSettings settings)
    {
        _dbAccessor = dbAccessor;
        _store = store;
        _logger = logger;
        _tablePrefix = settings["TablePrefix"];
    }

    public async Task<int> CreateAsync()
    {
        await using (var connection = _dbAccessor.CreateConnection())
        {
            await connection.OpenAsync();

            using (var transaction = await connection.BeginTransactionAsync())
            {
                _logger.LogDebug("Updating X (Twitter) Workflow Items");
                try
                {
                    var dialect = _store.Configuration.SqlDialect;
                    var documentTableName = _store.Configuration.TableNameConvention.GetDocumentTable();

                    var table = dialect.QuoteForTableName($"{_tablePrefix}{documentTableName}", _store.Configuration.Schema);
                    var contentColumn = dialect.QuoteForColumnName("Content");
                    var typeColumn = dialect.QuoteForColumnName("Type");

                    var updateCmd = $"UPDATE {table} SET {contentColumn} = REPLACE({contentColumn}, 'UpdateTwitterStatusTask', 'UpdateXTwitterStatusTask') WHERE {typeColumn} = 'OrchardCore.Workflows.Models.WorkflowType, OrchardCore.Workflows.Abstractions'";

                    await connection.ExecuteAsync(updateCmd, null, transaction);

                    await transaction.CommitAsync();

                    _logger.LogDebug("Updated X (Twitter) Workflow Items");
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(e, "An error occurred while updating X (Twitter) Workflow Items");

                    throw;
                }
            }
        }

        return 1;
    }
}

