using Microsoft.Data.Entity;
using OrchardVNext.Data.Providers;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.FileSystems.AppData;

namespace OrchardVNext.Data {
    public interface IDbContextFactoryHolder : IDependency {
        void Configure(DbContextOptions options);
    }

    public class DbContextFactoryHolder : IDbContextFactoryHolder {
        private readonly ShellSettings _shellSettings;
        private readonly IDataServicesProviderFactory _dataServicesProviderFactory;
        private readonly IAppDataFolder _appDataFolder;

        public DbContextFactoryHolder(
            ShellSettings shellSettings,
            IDataServicesProviderFactory dataServicesProviderFactory,
            IAppDataFolder appDataFolder) {
            _shellSettings = shellSettings;
            _dataServicesProviderFactory = dataServicesProviderFactory;
            _appDataFolder = appDataFolder;
        }

        public void Configure(DbContextOptions options) {
            var shellPath = _appDataFolder.Combine("Sites", _shellSettings.Name);
            _appDataFolder.CreateDirectory(shellPath);

            var shellFolder = _appDataFolder.MapPath(shellPath);

            _dataServicesProviderFactory.CreateProvider(
                new DataServiceParameters {
                    Provider = _shellSettings.DataProvider,
                    ConnectionString = _shellSettings.DataConnectionString,
                    DataFolder = shellFolder
                })
            .ConfigureContextOptions(options);
        }
    }
}