using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using YesSql;
using YesSql.Sql;
using YesSql.Sql.Schema;
using YesSql.Services;

namespace OrchardCore.Data.Migration;

public class RemoveSchemaBuilder : ISchemaBuilder
{
    private ICommandInterpreter _commandInterpreter;
    private readonly ILogger _logger;

    public string TablePrefix { get; private set; }
    public ISqlDialect Dialect { get; private set; }
    public ITableNameConvention TableNameConvention { get; private set; }
    public DbConnection Connection { get; set; }
    public DbTransaction Transaction { get; set; }
    public bool ThrowOnError { get; private set; }

    public HashSet<string> Tables { get; private set; } = new HashSet<string>();
    public HashSet<(string Name, Type Type, string Collection)> IndexTables { get; private set; } =
        new HashSet<(string Name, Type Type, string Collection)>();
    public HashSet<(string Name, Type Type, string Collection)> ReduceIndexTables { get; private set; } =
        new HashSet<(string Name, Type Type, string Collection)>();
    public HashSet<string> DocumentTables { get; private set; } = new HashSet<string>();
    public HashSet<string> BridgeTables { get; private set; } = new HashSet<string>();

    public RemoveSchemaBuilder(IConfiguration configuration, DbTransaction transaction, bool throwOnError = true)
    {
        Transaction = transaction;
        _logger = configuration.Logger;
        Connection = Transaction?.Connection;
        _commandInterpreter = configuration.CommandInterpreter;
        Dialect = configuration.SqlDialect;
        TablePrefix = configuration.TablePrefix;
        ThrowOnError = throwOnError;
        TableNameConvention = configuration.TableNameConvention;
    }

    public IEnumerable<string> GetTableNames()
    {
        return Tables.Select(Prefix)
            .Union(IndexTables.Select(i => Prefix(i.Name)))
            .Union(ReduceIndexTables.Select(i => Prefix(i.Name)))
            .Union(DocumentTables.Select(Prefix))
            .Append(Prefix(DbBlockIdGenerator.TableName))
            .Union(BridgeTables.Select(Prefix))
            ;
    }

    public ISchemaBuilder CreateTable(string name, Action<ICreateTableCommand> table)
    {
        Tables.Add(name);

        return this;
    }

    public ISchemaBuilder CreateMapIndexTable(Type indexType, Action<ICreateTableCommand> table, string collection)
    {
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
        var documentTable = TableNameConvention.GetDocumentTable(collection);

        IndexTables.Add((indexTable, indexType, collection));
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

    public ISchemaBuilder AlterTable(string name, Action<IAlterTableCommand> table) => this;
    public ISchemaBuilder AlterIndexTable(Type indexType, Action<IAlterTableCommand> table, string collection) => this;

    public ISchemaBuilder DropTable(string name)
    {
        Tables.Remove(name);

        return this;
    }

    public ISchemaBuilder DropMapIndexTable(Type indexType, string collection = null)
    {
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
        IndexTables.Remove((indexTable, indexType, collection));

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

    public void RemoveTables()
    {
        try
        {
            foreach (var name in Tables)
            {
                RemoveTable(name);
            }

            foreach (var name in DocumentTables)
            {
                RemoveTable(name);
            }

            var identifiersTable = new DropTableCommand(Prefix(DbBlockIdGenerator.TableName));
            Execute(_commandInterpreter.CreateSql(identifiersTable));
        }
        catch
        {
            if (ThrowOnError)
            {
                throw;
            }
        }
    }

    public void RemoveMapIndexTables()
    {
        try
        {
            foreach (var index in IndexTables)
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
            _logger.LogTrace(statement);
            Connection.Execute(statement, null, Transaction);
        }
    }

    private string Prefix(string table)
    {
        return TablePrefix + table;
    }
}
