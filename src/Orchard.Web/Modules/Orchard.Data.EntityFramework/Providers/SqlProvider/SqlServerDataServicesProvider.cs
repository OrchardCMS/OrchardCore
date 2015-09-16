using Microsoft.Data.Entity;

namespace Orchard.Data.EntityFramework.Providers.SqlProvider {
    [OrchardFeature("Orchard.Data.EntityFramework.SqlServer")]
    public class SqlServerDataServicesProvider : IDataServicesProvider {
        public string ProviderName {
            get { return "SqlServer"; }
        }

        public void ConfigureContextOptions(DbContextOptionsBuilder optionsBuilders, string connectionString) {
            optionsBuilders.UseSqlServer(connectionString);
        }
    }
}
