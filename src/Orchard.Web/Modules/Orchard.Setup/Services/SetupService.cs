using Orchard.Hosting;
using System;
using System.Linq;
using Microsoft.AspNet.Http;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Environment.Shell.Builders;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Models;
using Orchard.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Hosting.ShellBuilders;
using YesSql.Core.Services;
using Orchard.Environment.Shell.Descriptor;

namespace Orchard.Setup.Services
{
    public class SetupService : Component, ISetupService
    {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IExtensionManager _extensionManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRunningShellTable _runningShellTable;
        private readonly IRunningShellRouterTable _runningShellRouterTable;
        private readonly ILogger _logger;

        public SetupService(
            ShellSettings shellSettings,
            IOrchardHost orchardHost,
            IShellSettingsManager shellSettingsManager,
            IShellContainerFactory shellContainerFactory,
            ICompositionStrategy compositionStrategy,
            IExtensionManager extensionManager,
            IHttpContextAccessor httpContextAccessor,
            IRunningShellTable runningShellTable,
            IRunningShellRouterTable runningShellRouterTable,
            ILogger<SetupService> logger)
        {
            _shellSettings = shellSettings;
            _orchardHost = orchardHost;
            _shellSettingsManager = shellSettingsManager;
            _shellContainerFactory = shellContainerFactory;
            _compositionStrategy = compositionStrategy;
            _extensionManager = extensionManager;
            _httpContextAccessor = httpContextAccessor;
            _runningShellTable = runningShellTable;
            _runningShellRouterTable = runningShellRouterTable;
            _logger = logger;
        }

        public ShellSettings Prime()
        {
            return _shellSettings;
        }

        public string Setup(SetupContext context)
        {
            var initialState = _shellSettings.State;
            try
            {
                return SetupInternal(context);
            }
            catch
            {
                _shellSettings.State = initialState;
                throw;
            }
        }

        public string SetupInternal(SetupContext context)
        {
            string executionId;

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Running setup for tenant '{0}'.", _shellSettings.Name);
            }

            // Features to enable for Setup
            string[] hardcoded = {
                // Framework
                "Orchard.Hosting",
                // Core
                "Settings"
                };

            context.EnabledFeatures = hardcoded.Union(context.EnabledFeatures ?? Enumerable.Empty<string>()).Distinct().ToList();

            // Set shell state to "Initializing" so that subsequent HTTP requests are responded to with "Service Unavailable" while Orchard is setting up.
            _shellSettings.State = TenantState.Initializing;

            var shellSettings = new ShellSettings(_shellSettings);

            if (string.IsNullOrEmpty(shellSettings.DatabaseProvider))
            {
                shellSettings.DatabaseProvider = context.DatabaseProvider;
                shellSettings.ConnectionString = context.DatabaseConnectionString;
                shellSettings.TablePrefix = context.DatabaseTablePrefix;
            }

            // TODO: Add Encryption Settings in

            // Creating a standalone environment based on a "minimum shell descriptor".
            // In theory this environment can be used to resolve any normal components by interface, and those
            // components will exist entirely in isolation - no crossover between the safemode container currently in effect
            // It is used to initialize the database before the recipe is run.

            using (var environment = _orchardHost.CreateShellContext(shellSettings))
            {
                using (var scope = environment.CreateServiceScope())
                {
                    executionId = CreateTenantData(context, environment);

                    var store = scope.ServiceProvider.GetRequiredService<IStore>();
                    store.InitializeAsync();
                    
                    // Create the "minimum shell descriptor"
                    scope
                        .ServiceProvider
                        .GetService<IShellDescriptorManager>()
                        .UpdateShellDescriptorAsync(
                            0,
                            environment.Blueprint.Descriptor.Features,
                            environment.Blueprint.Descriptor.Parameters).Wait();
                }
            }

            shellSettings.State = TenantState.Running;
            _runningShellRouterTable.Remove(shellSettings.Name);
            _orchardHost.UpdateShellSettings(shellSettings);
            return executionId;
        }

        private string CreateTenantData(SetupContext context, ShellContext shellContext)
        {
            // Must mark state as Running - otherwise standalone enviro is created "for setup"
            return Guid.NewGuid().ToString();
        }
    }
}