using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Data.Migration
{
    /// <summary>
    /// Represents a tenant event that will be registered to OrchardShell.Activated in order to run migrations automatically.
    /// </summary>
    public class AutomaticDataMigrations : ModularTenantEvents
    {
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new instance of the <see cref="AutomaticDataMigrations"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="shellSettings">The <see cref="ShellSettings"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public AutomaticDataMigrations(
            IServiceProvider serviceProvider,
            ShellSettings shellSettings,
            ILogger<AutomaticDataMigrations> logger)
        {
            _serviceProvider = serviceProvider;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        /// <inheritdocs />
        public override Task ActivatingAsync()
        {
            if (!_shellSettings.IsUninitialized())
            {
                _logger.LogDebug("Executing data migrations for shell '{Name}'", _shellSettings.Name);

                var dataMigrationManager = _serviceProvider.GetService<IDataMigrationManager>();
                return dataMigrationManager.UpdateAllFeaturesAsync();
            }

            return Task.CompletedTask;
        }
    }
}
