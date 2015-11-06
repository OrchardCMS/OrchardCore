using Orchard.Environment.Extensions.Models;
using YesSql.Core.Sql;

namespace Orchard.Data.Migration
{
    public abstract class DataMigrations : IDataMigration
    {
        public Feature Feature { get; }
        public SchemaBuilder SchemaBuilder { get; set; }
    }
}
