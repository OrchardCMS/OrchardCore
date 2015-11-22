using Orchard.DependencyInjection;
using Orchard.Environment.Extensions.Models;
using YesSql.Core.Sql;

namespace Orchard.Data.Migration
{
    public interface IDataMigration : IDependency
    {
        SchemaBuilder SchemaBuilder { get; set; }
    }
}