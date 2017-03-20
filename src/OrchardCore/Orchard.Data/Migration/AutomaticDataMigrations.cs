using System;
using System.Threading.Tasks;
using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Tenant;

namespace Orchard.Data.Migration
{
    /// <summary>
    /// Registers to OrchardTenant.Activated in order to run migrations automatically
    /// </summary>
    public class AutomaticDataMigrations : IModularTenantEvents
    {
        private readonly TenantSettings _tenantSettings;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public AutomaticDataMigrations(
            IServiceProvider serviceProvider,
            TenantSettings tenantSettings,
            ILogger<AutomaticDataMigrations> logger)
        {
            _serviceProvider = serviceProvider;
            _tenantSettings = tenantSettings;
            _logger = logger;
        }

        public Task ActivatedAsync()
        {
            if (_tenantSettings.State != OrchardCore.Tenant.Models.TenantState.Uninitialized)
            {
                var dataMigrationManager = _serviceProvider.GetService<IDataMigrationManager>();
                return dataMigrationManager.UpdateAllFeaturesAsync();
            }

            return Task.CompletedTask;
        }

        public Task ActivatingAsync()
        {
            return Task.CompletedTask;
        }

        public Task TerminatingAsync()
        {
            return Task.CompletedTask;
        }

        public Task TerminatedAsync()
        {
            return Task.CompletedTask;
        }
    }
}