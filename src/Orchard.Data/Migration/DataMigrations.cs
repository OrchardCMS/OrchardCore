using Orchard.Environment.Extensions.Models;
using YesSql.Core.Sql;

namespace Orchard.Data.Migration
{
    public abstract class DataMigrations : IDataMigration
    {
        public SchemaBuilder SchemaBuilder { get; set; }
    }
}