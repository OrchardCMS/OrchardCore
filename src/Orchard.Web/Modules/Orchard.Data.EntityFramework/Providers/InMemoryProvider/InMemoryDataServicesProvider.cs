using Microsoft.Data.Entity;

namespace Orchard.Data.EntityFramework.Providers.SqlProvider {
    [OrchardFeature("Orchard.Data.EntityFramework.InMemory")]
    public class InMemoryDataServicesProvider : IDataServicesProvider {
        public string ProviderName {
            get { return "InMemory"; }
        }

        public void ConfigureContextOptions(DbContextOptionsBuilder optionsBuilders, string connectionString) {
            optionsBuilders.UseInMemoryDatabase();
        }
    }
}
