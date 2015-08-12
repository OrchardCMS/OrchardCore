using Microsoft.Data.Entity;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext.Data.EntityFramework.Providers {
    public interface IDataServicesProvider : ITransientDependency {
        string ProviderName { get; }
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