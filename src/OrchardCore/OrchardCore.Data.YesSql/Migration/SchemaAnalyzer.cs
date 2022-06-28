using System;
using System.Collections.Generic;
using System.Data.Common;
using YesSql;
using YesSql.Sql;
using YesSql.Sql.Schema;

namespace OrchardCore.Data.Migration;

public class SchemaAnalyzer : ISchemaBuilder
{
    public string TablePrefix { get; private set; }
    public ISqlDialect Dialect { get; private set; }
    public ITableNameConvention TableNameConvention { get; private set; }
    public DbConnection Connection { get; private set; }
    public DbTransaction Transaction { get; private set; }
    public bool ThrowOnError { get; private set; }

    public HashSet<string> Tables { get; private set; } = new HashSet<string>();
    public HashSet<string> IndexTables { get; private set; } = new HashSet<string>();
    public HashSet<string> BridgeTables { get; private set; } = new HashSet<string>();
    public HashSet<string> DocumentTables { get; private set; } = new HashSet<string>();
    public HashSet<string> Collections { get; private set; } = new HashSet<string>();

    public SchemaAnalyzer(IConfiguration configuration)
    {
        Dialect = configuration.SqlDialect;
        TablePrefix = configuration.TablePrefix;
        TableNameConvention = configuration.TableNameConvention;
    }

    private string Prefix(string table) => TablePrefix + table;

    public ISchemaBuilder CreateMapIndexTable(Type indexType, Action<ICreateTableCommand> table, string collection)
    {
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
        var documentTable = TableNameConvention.GetDocumentTable(collection);

        IndexTables.Add(Prefix(indexTable));
        DocumentTables.Add(Prefix(documentTable));

        if (!String.IsNullOrWhiteSpace(collection))
        {
            Collections.Add(collection);
        }

        return this;
    }

    public ISchemaBuilder CreateReduceIndexTable(Type indexType, Action<ICreateTableCommand> table, string collection = null)
    {
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
        var documentTable = TableNameConvention.GetDocumentTable(collection);
        var bridgeTableName = indexTable + "_" + documentTable;

        IndexTables.Add(Prefix(indexTable));
        DocumentTables.Add(Prefix(documentTable));
        BridgeTables.Add(Prefix(bridgeTableName));

        if (!String.IsNullOrWhiteSpace(collection))
        {
            Collections.Add(collection);
        }

        return this;
    }

    public ISchemaBuilder DropReduceIndexTable(Type indexType, string collection = null)
    {
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
        var documentTable = TableNameConvention.GetDocumentTable(collection);
        var bridgeTableName = indexTable + "_" + documentTable;

        IndexTables.Remove(Prefix(indexTable));
        BridgeTables.Remove(Prefix(bridgeTableName));

        return this;
    }

    public ISchemaBuilder DropMapIndexTable(Type indexType, string collection = null)
    {
        var indexTable = TableNameConvention.GetIndexTable(indexType, collection);

        IndexTables.Remove(Prefix(indexTable));

        return this;
    }

    public ISchemaBuilder CreateTable(string name, Action<ICreateTableCommand> table)
    {
        Tables.Add(Prefix(name));

        return this;
    }

    public ISchemaBuilder AlterTable(string name, Action<IAlterTableCommand> table) => this;

    public ISchemaBuilder AlterIndexTable(Type indexType, Action<IAlterTableCommand> table, string collection) => this;

    public ISchemaBuilder DropTable(string name)
    {
        Tables.Remove(Prefix(name));

        return this;
    }

    public ISchemaBuilder CreateForeignKey(string name, string srcTable, string[] srcColumns, string destTable, string[] destColumns)
        => this;

    public ISchemaBuilder DropForeignKey(string srcTable, string name) => this;
}
