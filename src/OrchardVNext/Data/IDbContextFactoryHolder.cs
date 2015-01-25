using Microsoft.Data.Entity;
using OrchardVNext.Data.Providers;
using OrchardVNext.Environment.Configuration;
using System;
using OrchardVNext.FileSystems.AppData;

namespace OrchardVNext.Data {
    public interface IDbContextFactoryHolder : ISingletonDependency {
        DbContextOptions BuildConfiguration();
    }

    public class DbContextFactoryHolder : IDbContextFactoryHolder, IDisposable {
        private readonly ShellSettings _shellSettings;
        private readonly IDataServicesProviderFactory _dataServicesProviderFactory;
        private readonly IAppDataFolder _appDataFolder;

        private DbContextOptions _dbContextOptions;

        public DbContextFactoryHolder(
            ShellSettings shellSettings,
            IDataServicesProviderFactory dataServicesProviderFactory,
            IAppDataFolder appDataFolder) {
            _shellSettings = shellSettings;
            _dataServicesProviderFactory = dataServicesProviderFactory;
            _appDataFolder = appDataFolder;
        }

        public DbContextOptions BuildConfiguration() {
            lock (this) {
                if (_dbContextOptions == null) {
                    var shellPath = _appDataFolder.Combine("Sites", _shellSettings.Name);
                    _appDataFolder.CreateDirectory(shellPath);

                    var shellFolder = _appDataFolder.MapPath(shellPath);

                    _dbContextOptions = _dataServicesProviderFactory.CreateProvider(
                        new DataServiceParameters {
                            Provider = _shellSettings.DataProvider,
                            ConnectionString = _shellSettings.DataConnectionString,
                            DataFolder = shellFolder
                        })
                .BuildContextOptions();
                }
            }
            return _dbContextOptions;
        }

        public void Dispose() {
        }
    }
}