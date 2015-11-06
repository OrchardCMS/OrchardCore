using Orchard.DependencyInjection;
using Orchard.Environment.Extensions.Models;
using YesSql.Core.Sql;

namespace Orchard.Data.Migration
{
    public interface IDataMigration : IDependency
    {
        Feature Feature { get; }
        SchemaBuilder SchemaBuilder { get; set; }
    }
}
