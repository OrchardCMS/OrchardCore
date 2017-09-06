using YesSql.Sql;

namespace OrchardCore.Data.Migration
{
    public interface IDataMigration
    {
        SchemaBuilder SchemaBuilder { get; set; }
    }
}