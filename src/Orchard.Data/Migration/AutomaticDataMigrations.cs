using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell;

namespace Orchard.Data.Migration
{
    /// <summary>
    /// Registers to OrchardShell.Activated in order to run migrations automatically
    /// </summary>
    public class AutomaticDataMigrations : IOrchardShellEvents
    {
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IFeatureManager _featureManager;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public AutomaticDataMigrations(
            IDataMigrationManager dataMigrationManager,
            IFeatureManager featureManager,
            ShellSettings shellSettings,
            ILogger<AutomaticDataMigrations> logger)
        {
            _dataMigrationManager = dataMigrationManager;
            _featureManager = featureManager;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public Task ActivatedAsync()
        {
            return _dataMigrationManager.UpdateAllFeaturesAsync();
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