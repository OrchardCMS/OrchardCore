using Microsoft.Data.Entity;
using OrchardVNext.Data.Providers;
using OrchardVNext.Environment.Configuration;
using System;

namespace OrchardVNext.Data {
    public interface IDbContextFactoryHolder : ISingletonDependency {
        DbContextOptions BuildConfiguration();
    }

    public class DbContextFactoryHolder : IDbContextFactoryHolder, IDisposable {
        private readonly ShellSettings _shellSettings;
        private readonly IDataServicesProviderFactory _dataServicesProviderFactory;

        public DbContextFactoryHolder(
            ShellSettings shellSettings,
            IDataServicesProviderFactory dataServicesProviderFactory) {
            _shellSettings = shellSettings;
            _dataServicesProviderFactory = dataServicesProviderFactory;
        }

        public DbContextOptions BuildConfiguration() {
            _dataServicesProviderFactory.CreateProvider(
                new DataServiceParameters {


                }
                );
            return null;
        }

        public void Dispose() {
        }
    }
}