using YesSql.Core.Sql;

namespace Orchard.Data.Migration
{
    public interface IDataMigration
    {
        SchemaBuilder SchemaBuilder { get; set; }
    }
}