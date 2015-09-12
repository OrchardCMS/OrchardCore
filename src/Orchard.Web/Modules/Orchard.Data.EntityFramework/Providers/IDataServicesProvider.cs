using Microsoft.Data.Entity;

namespace Orchard.Data.EntityFramework.Providers {
    public interface IDataServicesProvider : IContentStoreDataProvider {
        void ConfigureContextOptions(DbContextOptionsBuilder optionsBuilders,string connectionString);
    }

    public class SqlServerDataServicesProvider : IDataServicesProvider {
        public string ProviderName {
            get { return "SqlServer"; }
        }

        public void ConfigureContextOptions(DbContextOptionsBuilder optionsBuilders,string connectionString) {
            optionsBuilders.UseSqlServer(connectionString);
        }
    }

    public class InMemoryDataServicesProvider : IDataServicesProvider {
        public string ProviderName {
            get { return "InMemory"; }
        }

        public void ConfigureContextOptions(DbContextOptionsBuilder optionsBuilders,string connectionString) {
            optionsBuilders.UseInMemoryDatabase(persist: true);
        }
    }
}