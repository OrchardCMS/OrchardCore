using Orchard.DependencyInjection;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Data.Migration {
    public interface IDataMigration : IDependency {
        Feature Feature { get; }
    }
}
