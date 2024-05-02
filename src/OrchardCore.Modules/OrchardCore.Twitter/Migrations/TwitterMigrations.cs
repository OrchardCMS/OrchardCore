using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using YesSql;

namespace OrchardCore.Twitter.Migrations;

public class TwitterMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IStore _store;
    private readonly ShellSettings _shellSettings;

    public TwitterMigrations(IContentDefinitionManager contentDefinitionManager, IStore store, ShellSettings shellSettings)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _store = store;
        _shellSettings = shellSettings;
    }

    public int Create()
    {
        return 2;
    }

    public int UpdateFrom1()
    {
        using var connection = _store.Configuration.ConnectionFactory.CreateConnection();
        var tablePrefix = string.IsNullOrEmpty(_store.Configuration.TablePrefix) ? "" : _store.Configuration.TablePrefix + "_";
        var sql = $@"
                -- Updates Existing Workflows
                UPDATE {tablePrefix}Document SET Content = REPLACE(content, 'UpdateTwitterStatusTask', 'UpdateXStatusTask')
                WHERE Type = 'OrchardCore.Workflows.Models.WorkflowType, OrchardCore.Workflows.Abstractions';
                ";

        using var transaction = connection.BeginTransaction();
        using var command = transaction.Connection.CreateCommand();

        command.CommandText = sql;
        command.CommandType = System.Data.CommandType.Text;

        command.ExecuteNonQuery();

        return 2;
    }


}
