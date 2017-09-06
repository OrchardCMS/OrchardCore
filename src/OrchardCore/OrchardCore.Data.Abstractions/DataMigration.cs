using YesSql.Sql;

namespace OrchardCore.Data.Migration
{
    public abstract class DataMigration : IDataMigration
    {
        public SchemaBuilder SchemaBuilder { get; set; }
    }
}
