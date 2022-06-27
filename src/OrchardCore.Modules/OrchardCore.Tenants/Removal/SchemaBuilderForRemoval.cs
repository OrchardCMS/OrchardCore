using System;
using System.Collections.Generic;
using System.Data.Common;
using YesSql;
using YesSql.Sql;
using YesSql.Sql.Schema;

namespace OrchardCore.Tenants.Removal
{
    public class SchemaBuilderForRemoval : ISchemaBuilder
    {
        public string TablePrefix { get; private set; }
        public ISqlDialect Dialect { get; private set; }
        public ITableNameConvention TableNameConvention { get; private set; }
        public DbConnection Connection { get; private set; }
        public DbTransaction Transaction { get; private set; }
        public bool ThrowOnError { get; private set; }

        public HashSet<string> IndexTables { get; private set; } = new HashSet<string>();
        public HashSet<string> DocumentTables { get; private set; } = new HashSet<string>();
        public HashSet<string> BridgeTables { get; private set; } = new HashSet<string>();
        public HashSet<string> Tables { get; private set; } = new HashSet<string>();

        public SchemaBuilderForRemoval(IConfiguration configuration)
        {
            Dialect = configuration.SqlDialect;
            TablePrefix = configuration.TablePrefix;
            TableNameConvention = configuration.TableNameConvention;
        }

        private string Prefix(string table)
        {
            return TablePrefix + table;
        }

        public ISchemaBuilder CreateMapIndexTable(Type indexType, Action<ICreateTableCommand> table, string collection)
        {
            var indexTable = TableNameConvention.GetIndexTable(indexType, collection);
            var documentTable = TableNameConvention.GetDocumentTable(collection);

            IndexTables.Add(Prefix(indexTable));
            DocumentTables.Add(Prefix(documentTable));

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

            return this;
        }

        public ISchemaBuilder DropReduceIndexTable(Type indexType, string collection = null)
        {
            return this;
        }

        public ISchemaBuilder DropMapIndexTable(Type indexType, string collection = null)
        {
            return this;
        }

        public ISchemaBuilder CreateTable(string name, Action<ICreateTableCommand> table)
        {
            Tables.Add(Prefix(name));

            return this;
        }

        public ISchemaBuilder AlterTable(string name, Action<IAlterTableCommand> table)
        {
            return this;
        }

        public ISchemaBuilder AlterIndexTable(Type indexType, Action<IAlterTableCommand> table, string collection)
        {
            return this;
        }

        public ISchemaBuilder DropTable(string name)
        {
            return this;
        }

        public ISchemaBuilder CreateForeignKey(string name, string srcTable, string[] srcColumns, string destTable, string[] destColumns)
        {
            return this;
        }

        public ISchemaBuilder DropForeignKey(string srcTable, string name)
        {
            return this;
        }
    }
}
