using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Data.Migration
{
    /// <summary>
    /// Registers to OrchardShell.Activated in order to run migrations automatically
    /// </summary>
    public class AutomaticDataMigrations : ModularTenantEvents
    {
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public AutomaticDataMigrations(
            IServiceProvider serviceProvider,
            ShellSettings shellSettings,
            ILogger<AutomaticDataMigrations> logger)
        {
            _serviceProvider = serviceProvider;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public override Task ActivatingAsync()
        {
            if (_shellSettings.State != Environment.Shell.Models.TenantState.Uninitialized)
            {
                _logger.LogDebug("Executing data migrations");

                var dataMigrationManager = _serviceProvider.GetService<IDataMigrationManager>();
                return dataMigrationManager.UpdateAllFeaturesAsync();
            }

            return Task.CompletedTask;
        }
    }
}