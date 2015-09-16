using Microsoft.Data.Entity;

namespace Orchard.Data.EntityFramework.Providers {
    public interface IDataServicesProvider : IContentStoreDataProvider {
        void ConfigureContextOptions(DbContextOptionsBuilder optionsBuilders,string connectionString);
    }
}