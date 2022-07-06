using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Dapper;
using YesSql;
using YesSql.Services;
using YesSql.Sql;
using YesSql.Sql.Schema;

namespace OrchardCore.Environment.Shell.Removing;

internal class ShellDbTablesInfo : ISchemaBuilder
{
    private ICommandInterpreter _commandInterpreter;

    public string TenantName { get; set; }
    public string TenantTablePrefix { get; set; }
    public string DatabaseProvider { get; set; }
    public string TablePrefix { get; set; }
    public IEnumerable<string> TableNames { get; set; } = Enumerable.Empty<string>();
    public bool Success => ErrorMessage == null;
    public string ErrorMessage { get; set; }

    public ISqlDialect Dialect { get; private set; }
    public ITableNameConvention TableNameConvention { get; private set; }
    public DbConnection Connection { get; set; }
    public DbTransaction Transaction { get; set; }
    public bool ThrowOnError { get; private set; }

    public HashSet<(string Name, Type Type, string Collection)> MapIndexTables { get; private set; } =
        new HashSet<(string Name, Type Type, string Collection)>();

    public HashSet<(string Name, Type Type, string Collection)> ReduceIndexTables { get; private set; } =
        new HashSet<(string Name, Type Type, string Collection)>();

    public HashSet<string> BridgeTables { get; private set; } = new HashSet<string>();
    public HashSet<string> DocumentTables { get; private set; } = new HashSet<string>();
    public HashSet<string> Tables { get; private set; } = new HashSet<string>();

    public ShellDbTablesInfo Configure(string tenant)
    {
        TenantName = tenant;

        return this;
    }

    public ShellDbTablesInfo Configure(ShellSettings shellSettings)
    {
        TenantName = shellSettings.Name;
        TenantTablePrefix = shellSettings["TablePrefix"];
        DatabaseProvider = shellSettings["DatabaseProvider"];

        return this;
    }

    public ShellDbTablesInfo Configure(IConfiguration configuration)
    {
        Dialect = configuration.SqlDialect;
        TablePrefix = configuration.TablePrefix;
        TableNameConvention = configuration.TableNameConvention;
        _commandInterpreter = configuration.CommandInterpreter;

        return this;
    }

    public ShellDbTablesInfo Configure(DbTransaction transaction, bool throwOnError = true)
    {
        Transaction = transaction;
        Connection = transaction.Connection;
        ThrowOnError = throwOnError;

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
        try
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
        catch
        {
            if (ThrowOnError)
            {
                throw;
            }
        }
    }

    public void RemoveReduceIndexTables()
    {
        try
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
        catch
        {
            if (ThrowOnError)
            {
                throw;
            }
        }
    }

    public void RemoveDocumentTables()
    {
        try
        {
            foreach (var name in DocumentTables)
            {
                RemoveTable(name);
            }

            RemoveTable(DbBlockIdGenerator.TableName);
        }
        catch
        {
            if (ThrowOnError)
            {
                throw;
            }
        }
    }

    public void RemoveTables()
    {
        try
        {
            foreach (var name in Tables)
            {
                RemoveTable(name);
            }
        }
        catch
        {
            if (ThrowOnError)
            {
                throw;
            }
        }
    }

    public void RemoveTable(string name)
    {
        try
        {
            var deleteTable = new DropTableCommand(Prefix(name));
            Execute(_commandInterpreter.CreateSql(deleteTable));
        }
        catch
        {
            if (ThrowOnError)
            {
                throw;
            }
        }
    }

    public ISchemaBuilder CreateForeignKey(string name, string srcTable, string[] srcColumns, string destTable, string[] destColumns)
    {
        try
        {
            var command = new CreateForeignKeyCommand(Dialect.FormatKeyName(Prefix(name)), Prefix(srcTable), srcColumns, Prefix(destTable), destColumns);
            var sql = _commandInterpreter.CreateSql(command);
            Execute(sql);
        }
        catch
        {
            if (ThrowOnError)
            {
                throw;
            }
        }

        return this;
    }

    public ISchemaBuilder DropForeignKey(string srcTable, string name)
    {
        try
        {
            var command = new DropForeignKeyCommand(Dialect.FormatKeyName(Prefix(srcTable)), Prefix(name));
            Execute(_commandInterpreter.CreateSql(command));
        }
        catch
        {
            if (ThrowOnError)
            {
                throw;
            }
        }

        return this;
    }

    private void Execute(IEnumerable<string> statements)
    {
        foreach (var statement in statements)
        {
            Connection.Execute(statement, null, Transaction);
        }
    }

    private string Prefix(string table) => TablePrefix + table;
}
