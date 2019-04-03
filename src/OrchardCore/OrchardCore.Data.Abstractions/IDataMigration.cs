using YesSql.Sql;

namespace OrchardCore.Data.Migration
{
    public interface IDataMigration
    {
        ISchemaBuilder SchemaBuilder { get; set; }
    }
}