using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using YesSql;
using YesSql.Services;
using YesSql.Sql;
using YesSql.Sql.Schema;

namespace OrchardCore.Environment.Shell.Removing;

internal class ShellDbTablesInfo : ISchemaBuilder
{
    private ICommandInterpreter _commandInterpreter;
    public string TablePrefix { get; set; }
    public IEnumerable<string> TableNames { get; set; } = Enumerable.Empty<string>();
    public ISqlDialect Dialect { get; private set; }
    public ITableNameConvention TableNameConvention { get; private set; }
    public DbConnection Connection { get; set; }
    public DbTransaction Transaction { get; set; }
    public bool ThrowOnError { get; private set; }
    public ILogger _logger { get; set; } = NullLogger.Instance;

    public HashSet<(string Name, Type Type, string Collection)> MapIndexTables { get; private set; } =
        new HashSet<(string Name, Type Type, string Collection)>();

    public HashSet<(string Name, Type Type, string Collection)> ReduceIndexTables { get; private set; } =
        new HashSet<(string Name, Type Type, string Collection)>();

    public HashSet<string> BridgeTables { get; private set; } = new HashSet<string>();
    public HashSet<string> DocumentTables { get; private set; } = new HashSet<string>();
    public HashSet<string> Tables { get; private set; } = new HashSet<string>();

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
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
        var documentTable = TableNameConvention.GetDocumentTable(collection);

        MapIndexTables.Add((indexTable, indexType, collection));
        DocumentTables.Add(documentTable);

        return this;
    }

    public ISchemaBuilder CreateReduceIndexTable(Type indexType, Action<ICreateTableCommand> table, string collection = null)
    {
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
        var documentTable = TableNameConvention.GetDocumentTable(collection);
        var bridgeTable = indexTable + "_" + documentTable;

        ReduceIndexTables.Add((indexTable, indexType, collection));
        DocumentTables.Add(documentTable);
        BridgeTables.Add(bridgeTable);

        return this;
    }

    public ISchemaBuilder CreateTable(string name, Action<ICreateTableCommand> table)
    {
        Tables.Add(name);

        return this;
    }

    public ISchemaBuilder AlterIndexTable(Type indexType, Action<IAlterTableCommand> table, string collection) => this;
    public ISchemaBuilder AlterTable(string name, Action<IAlterTableCommand> table) => this;

    public ISchemaBuilder DropMapIndexTable(Type indexType, string collection = null)
    {
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
        MapIndexTables.Remove((indexTable, indexType, collection));

        return this;
    }

    public ISchemaBuilder DropReduceIndexTable(Type indexType, string collection = null)
    {
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
        var documentTable = TableNameConvention.GetDocumentTable(collection);
        var bridgeTable = indexTable + "_" + documentTable;

        ReduceIndexTables.Remove((indexTable, indexType, collection));
        BridgeTables.Remove(bridgeTable);

        return this;
    }

    public ISchemaBuilder DropTable(string name)
    {
        Tables.Remove(name);

        return this;
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

            if (String.IsNullOrEmpty(Dialect.CascadeConstraintsString))
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
            if (String.IsNullOrEmpty(Dialect.CascadeConstraintsString))
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
        DocumentTables.Add(TableNameConvention.GetDocumentTable(String.Empty));

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

    public ISchemaBuilder CreateForeignKey(string name, string srcTable, string[] srcColumns, string destTable, string[] destColumns) => this;

    public ISchemaBuilder DropForeignKey(string srcTable, string name)
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

        return this;
    }

    public ISchemaBuilder CreateSchema(string schema) => this;

    private void Execute(IEnumerable<string> statements)
    {
        foreach (var statement in statements)
        {
            Connection.Execute(statement, null, Transaction);
        }
    }

    private string Prefix(string table) => TablePrefix + table;
}
