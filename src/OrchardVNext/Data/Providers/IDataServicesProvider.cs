using Microsoft.Data.Entity;

namespace OrchardVNext.Data.Providers {
    public interface IDataServicesProvider : ITransientDependency {
        string ProviderName { get; }
        void ConfigureContextOptions(DbContextOptions options);
    }

    public class SqlServerDataServicesProvider : IDataServicesProvider {
        public string ProviderName {
            get { return "SqlServer"; }
        }

        public void ConfigureContextOptions(DbContextOptions options) {
            options.UseSqlServer(@"");
        }
    }

    public class InMemoryDataServicesProvider : IDataServicesProvider {
        public string ProviderName {
            get { return "InMemory"; }
        }

        public void ConfigureContextOptions(DbContextOptions options) {
            options.UseInMemoryStore(persist: true);
        }
    }
}