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
using Orchard.Hosting.ShellBuilders;
using YesSql.Core.Services;

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
            ILoggerFactory loggerFactory)
        {
            _shellSettings = shellSettings;
            _orchardHost = orchardHost;
            _shellSettingsManager = shellSettingsManager;
            _shellContainerFactory = shellContainerFactory;
            _compositionStrategy = compositionStrategy;
            _extensionManager = extensionManager;
            _httpContextAccessor = httpContextAccessor;
            _runningShellTable = runningShellTable;
            _logger = loggerFactory.CreateLogger<SetupService>();
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

            _logger.LogInformation("Running setup for tenant '{0}'.", _shellSettings.Name);

            // The vanilla Orchard distibution has the following features enabled.
            string[] hardcoded = {
                // Framework
                "Orchard.Hosting",
                // Core
                "Settings",
                // Test Modules
                "Orchard.Demo", "Orchard.Test1"
                };

            context.EnabledFeatures = hardcoded.Union(context.EnabledFeatures ?? Enumerable.Empty<string>()).Distinct().ToList();

            // Set shell state to "Initializing" so that subsequent HTTP requests are responded to with "Service Unavailable" while Orchard is setting up.
            _shellSettings.State = TenantState.Initializing;

            var shellSettings = new ShellSettings(_shellSettings);
            shellSettings.DatabaseProvider = context.DatabaseProvider;
            shellSettings.ConnectionString = context.DatabaseConnectionString;
            shellSettings.TablePrefix = context.DatabaseTablePrefix;

            //if (shellSettings.DataProviders.Any()) {
            //    DataProvider provider = new DataProvider();
            //shellSettings.DataProvider = context.DatabaseProvider;
            //shellSettings.DataConnectionString = context.DatabaseConnectionString;
            //shellSettings.DataTablePrefix = context.DatabaseTablePrefix;
            //}

            // TODO: Add Encryption Settings in

            var shellDescriptor = new ShellDescriptor
            {
                Features = context.EnabledFeatures.Select(name => new ShellFeature { Name = name }).ToList()
            };

            // creating a standalone environment.
            // in theory this environment can be used to resolve any normal components by interface, and those
            // components will exist entirely in isolation - no crossover between the safemode container currently in effect

            using (var environment = _orchardHost.CreateShellContext(shellSettings))
            {
                executionId = CreateTenantData(context, environment);

                using (var store = (IStore)environment.ServiceProvider.GetService(typeof(IStore)))
                {
                    store.InitializeAsync();
                }
            }

            shellSettings.State = TenantState.Running;
            _orchardHost.UpdateShellSettings(shellSettings);
            return executionId;
        }

        private string CreateTenantData(SetupContext context, ShellContext shellContext)
        {
            // must mark state as Running - otherwise standalone enviro is created "for setup"


            return Guid.NewGuid().ToString();
        }
    }
}