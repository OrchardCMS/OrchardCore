using YesSql.Sql;

namespace Orchard.Data.Migration
{
    public interface IDataMigration
    {
        SchemaBuilder SchemaBuilder { get; set; }
    }
}