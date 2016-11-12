using YesSql.Core.Sql;

namespace Orchard.Data.Migration
{
    public abstract class DataMigration : IDataMigration
    {
        public SchemaBuilder SchemaBuilder { get; set; }
    }
}
