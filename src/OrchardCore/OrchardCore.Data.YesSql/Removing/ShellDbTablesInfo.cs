using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using YesSql;
using YesSql.Services;
using YesSql.Sql;
using YesSql.Sql.Schema;

namespace OrchardCore.Environment.Shell.Removing;

internal sealed class ShellDbTablesInfo : ISchemaBuilder
{
    private ICommandInterpreter _commandInterpreter;
    public string TablePrefix { get; set; }
    public IEnumerable<string> TableNames { get; set; } = [];
    public ISqlDialect Dialect { get; private set; }
    public ITableNameConvention TableNameConvention { get; private set; }
    public DbConnection Connection { get; set; }
    public DbTransaction Transaction { get; set; }
    public bool ThrowOnError { get; private set; }
    public ILogger _logger { get; set; } = NullLogger.Instance;

    public HashSet<(string Name, Type Type, string Collection)> MapIndexTables { get; private set; } =
        [];

    public HashSet<(string Name, Type Type, string Collection)> ReduceIndexTables { get; private set; } =
        [];

    public HashSet<string> BridgeTables { get; private set; } = [];
    public HashSet<string> DocumentTables { get; private set; } = [];
    public HashSet<string> Tables { get; private set; } = [];

    public ShellDbTablesInfo Configure(IConfiguration configuration)
    {
        Dialect = configuration.SqlDialect;
        TablePrefix = configuration.TablePrefix;
        TableNameConvention = configuration.TableNameConvention;
        _commandInterpreter = configuration.CommandInterpreter;

        return this;
    }

    public ShellDbTablesInfo Configure(DbTransaction transaction, ILogger logger, bool throwOnError = true)
    {
        Transaction = transaction;
        Connection = transaction.Connection;
        ThrowOnError = throwOnError;
        _logger = logger;

        return this;
    }

    public IEnumerable<string> GetTableNames()
    {
        return MapIndexTables.Select(i => Prefix(i.Name))
            .Union(ReduceIndexTables.Select(i => Prefix(i.Name)))
            .Union(BridgeTables.Select(Prefix))
            .Append(Prefix(DbBlockIdGenerator.TableName))
            .Union(DocumentTables.Select(Prefix))
            .Union(Tables.Select(Prefix))
            .ToArray();
    }

    public ISchemaBuilder CreateMapIndexTable(Type indexType, Action<ICreateTableCommand> table, string collection)
    {
        CreateMapIndexTableAsync(indexType, table, collection).GetAwaiter().GetResult();
        return this;
    }

    public Task CreateMapIndexTableAsync(Type indexType, Action<ICreateTableCommand> table, string collection)
    {
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
        var documentTable = TableNameConvention.GetDocumentTable(collection);

        MapIndexTables.Add((indexTable, indexType, collection));
        DocumentTables.Add(documentTable);

        return Task.CompletedTask;
    }

    public ISchemaBuilder CreateReduceIndexTable(Type indexType, Action<ICreateTableCommand> table, string collection = null)
    {
        CreateReduceIndexTableAsync(indexType, table, collection).GetAwaiter().GetResult();

        return this;
    }

    public Task CreateReduceIndexTableAsync(Type indexType, Action<ICreateTableCommand> table, string collection = null)
    {
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
        var documentTable = TableNameConvention.GetDocumentTable(collection);
        var bridgeTable = indexTable + "_" + documentTable;

        ReduceIndexTables.Add((indexTable, indexType, collection));
        DocumentTables.Add(documentTable);
        BridgeTables.Add(bridgeTable);

        return Task.CompletedTask;
    }

    public ISchemaBuilder CreateTable(string name, Action<ICreateTableCommand> table)
    {
        CreateTableAsync(name, table).GetAwaiter().GetResult();

        return this;
    }

    public Task CreateTableAsync(string name, Action<ICreateTableCommand> table)
    {
        Tables.Add(name);

        return Task.CompletedTask;
    }

    public ISchemaBuilder AlterIndexTable(Type indexType, Action<IAlterTableCommand> table, string collection)
        => this;

    public Task AlterIndexTableAsync(Type indexType, Action<IAlterTableCommand> table, string collection)
        => Task.CompletedTask;

    public ISchemaBuilder AlterTable(string name, Action<IAlterTableCommand> table)
        => this;

    public Task AlterTableAsync(string name, Action<IAlterTableCommand> table)
        => Task.CompletedTask;

    public ISchemaBuilder DropMapIndexTable(Type indexType, string collection = null)
    {
        DropMapIndexTableAsync(indexType, collection).GetAwaiter().GetResult();

        return this;
    }

    public Task DropMapIndexTableAsync(Type indexType, string collection = null)
    {
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
        MapIndexTables.Remove((indexTable, indexType, collection));

        return Task.CompletedTask;
    }

    public ISchemaBuilder DropReduceIndexTable(Type indexType, string collection = null)
    {
        DropReduceIndexTableAsync(indexType, collection).GetAwaiter().GetResult();

        return this;
    }

    public Task DropReduceIndexTableAsync(Type indexType, string collection = null)
    {
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
        var documentTable = TableNameConvention.GetDocumentTable(collection);
        var bridgeTable = indexTable + "_" + documentTable;

        ReduceIndexTables.Remove((indexTable, indexType, collection));
        BridgeTables.Remove(bridgeTable);

        return Task.CompletedTask;
    }

    public ISchemaBuilder DropTable(string name)
    {
        DropTableAsync(name).GetAwaiter().GetResult();

        return this;
    }

    public Task DropTableAsync(string name)
    {
        Tables.Remove(name);

        return Task.CompletedTask;
    }

    public void RemoveAllTables()
    {
        RemoveMapIndexTables();
        RemoveReduceIndexTables();
        RemoveDocumentTables();
        RemoveTables();
    }

    public void RemoveMapIndexTables()
    {
        foreach (var index in MapIndexTables)
        {
            var indexTypeName = index.Type.Name;
            var indexTable = TableNameConvention.GetIndexTable(index.Type, index.Collection);

            if (string.IsNullOrEmpty(Dialect.CascadeConstraintsString))
            {
                DropForeignKey(indexTable, "FK_" + (index.Collection ?? "") + indexTypeName);
            }

            RemoveTable(indexTable);
        }
    }

    public void RemoveReduceIndexTables()
    {
        foreach (var index in ReduceIndexTables)
        {
            var indexTable = TableNameConvention.GetIndexTable(index.Type, index.Collection);
            var documentTable = TableNameConvention.GetDocumentTable(index.Collection);

            var bridgeTable = indexTable + "_" + documentTable;
            if (string.IsNullOrEmpty(Dialect.CascadeConstraintsString))
            {
                DropForeignKey(bridgeTable, "FK_" + bridgeTable + "_Id");
                DropForeignKey(bridgeTable, "FK_" + bridgeTable + "_DocumentId");
            }

            RemoveTable(bridgeTable);
            RemoveTable(indexTable);
        }
    }

    public void RemoveDocumentTables()
    {
        // Always try to remove the main 'Document' table that may have been
        // auto created on 'YesSql' side, even if no document was persisted.
        DocumentTables.Add(TableNameConvention.GetDocumentTable(string.Empty));

        foreach (var name in DocumentTables)
        {
            RemoveTable(name);
        }

        RemoveTable(DbBlockIdGenerator.TableName);
    }

    public void RemoveTables()
    {
        foreach (var name in Tables)
        {
            RemoveTable(name);
        }
    }

    public void RemoveTable(string name)
    {
        try
        {
            var deleteTable = new DropTableCommand(Prefix(name));
            Execute(_commandInterpreter.CreateSql(deleteTable));
        }
        catch (Exception ex)
        {
            if (ThrowOnError)
            {
                throw;
            }

            _logger.LogError(ex, "Failed to remove table {TableName}.", Prefix(name));
        }
    }

    public ISchemaBuilder CreateForeignKey(string name, string srcTable, string[] srcColumns, string destTable, string[] destColumns)
        => this;

    public Task CreateForeignKeyAsync(string name, string srcTable, string[] srcColumns, string destTable, string[] destColumns)
        => Task.CompletedTask;

    public ISchemaBuilder DropForeignKey(string srcTable, string name)
    {
        DropForeignKeyAsync(srcTable, name).GetAwaiter().GetResult();

        return this;
    }

    public Task DropForeignKeyAsync(string srcTable, string name)
    {
        try
        {
            var command = new DropForeignKeyCommand(Dialect.FormatKeyName(Prefix(srcTable)), Prefix(name));
            Execute(_commandInterpreter.CreateSql(command));
        }
        catch (Exception ex)
        {
            if (ThrowOnError)
            {
                throw;
            }

            _logger.LogError(ex, "Failed to drop foreign key {KeyName}.", Prefix(name));
        }

        return Task.CompletedTask;
    }

    public ISchemaBuilder CreateSchema(string schema)
        => this;

    public Task CreateSchemaAsync(string schema)
        => Task.CompletedTask;

    private void Execute(IEnumerable<string> statements)
    {
        foreach (var statement in statements)
        {
            Connection.Execute(statement, null, Transaction);
        }
    }

    private string Prefix(string table) => TablePrefix + table;
}
