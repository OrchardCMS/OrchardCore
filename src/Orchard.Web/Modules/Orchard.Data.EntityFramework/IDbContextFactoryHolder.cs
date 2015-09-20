using Microsoft.Data.Entity;
using Orchard.Data.EntityFramework.Providers;
using Orchard.Environment.Shell;
using Orchard.FileSystem.AppData;
using System.Collections.Generic;

namespace Orchard.Data.EntityFramework {
    public interface IDbContextFactoryHolder {
        void Configure(DbContextOptionsBuilder optionsBuilder);
    }

    [OrchardFeature("Orchard.Data.EntityFramework")]
    public class DbContextFactoryHolder : IDbContextFactoryHolder {
        private readonly ShellSettings _shellSettings;
        private readonly IEnumerable<IDataServicesProvider> _dataServicesProviders;
        private readonly IAppDataFolder _appDataFolder;

        public DbContextFactoryHolder(
            ShellSettings shellSettings,
            IEnumerable<IDataServicesProvider> dataServicesProviders,
            IAppDataFolder appDataFolder) {
            _shellSettings = shellSettings;
            _dataServicesProviders = dataServicesProviders;
            _appDataFolder = appDataFolder;
        }

        public void Configure(DbContextOptionsBuilder optionsBuilders) {
            var shellPath = _appDataFolder.Combine("Sites", _shellSettings.Name);
            _appDataFolder.CreateDirectory(shellPath);

            var shellFolder = _appDataFolder.MapPath(shellPath);

            foreach(var provider in _dataServicesProviders) {
                provider.ConfigureContextOptions(optionsBuilders, string.Empty);
            }
        }
    }
}