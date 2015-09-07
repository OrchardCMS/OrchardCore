using Microsoft.Data.Entity;
using Orchard.Configuration.Environment;
using Orchard.Data.EntityFramework.Providers;
using Orchard.FileSystem.AppData;

namespace Orchard.Data.EntityFramework {
    public interface IDbContextFactoryHolder {
        void Configure(DbContextOptionsBuilder optionsBuilder);
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

        public void Configure(DbContextOptionsBuilder optionsBuilders) {
            var shellPath = _appDataFolder.Combine("Sites", _shellSettings.Name);
            _appDataFolder.CreateDirectory(shellPath);

            var shellFolder = _appDataFolder.MapPath(shellPath);
            
            _dataServicesProviderFactory.CreateProvider(
                new DataServiceParameters {
                    Provider = _shellSettings.DataProvider,
                    ConnectionString = _shellSettings.DataConnectionString,
                    DataFolder = shellFolder
                })
            .ConfigureContextOptions(optionsBuilders,_shellSettings.DataConnectionString);
        }
    }
}