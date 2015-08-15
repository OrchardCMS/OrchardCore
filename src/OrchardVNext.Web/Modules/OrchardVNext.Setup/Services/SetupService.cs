using Microsoft.AspNet.Hosting;
using OrchardVNext.Environment;
using OrchardVNext.Environment.Descriptor.Models;
using OrchardVNext.Environment.Extensions;
using OrchardVNext.Environment.ShellBuilders;
using System;
using System.Linq;
using Microsoft.AspNet.Http;
using OrchardVNext.Abstractions.Localization;
using OrchardVNext.Configuration.Environment;

namespace OrchardVNext.Setup.Services {
    public class SetupService : ISetupService {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IExtensionManager _extensionManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SetupService(
            ShellSettings shellSettings,
            IOrchardHost orchardHost,
            IShellSettingsManager shellSettingsManager,
            IShellContainerFactory shellContainerFactory,
            ICompositionStrategy compositionStrategy,
            IExtensionManager extensionManager,
            IHttpContextAccessor httpContextAccessor) {
            _shellSettings = shellSettings;
            _orchardHost = orchardHost;
            _shellSettingsManager = shellSettingsManager;
            _shellContainerFactory = shellContainerFactory;
            _compositionStrategy = compositionStrategy;
            _extensionManager = extensionManager;
            _httpContextAccessor = httpContextAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ShellSettings Prime() {
            return _shellSettings;
        }

        public string Setup(SetupContext context) {
            string executionId = Guid.NewGuid().ToString();

            // The vanilla Orchard distibution has the following features enabled.
            string[] hardcoded = {
                // Framework
                "OrchardVNext.Framework",
                // Core
                "Settings",
                // Test Modules
                "OrchardVNext.Demo", "OrchardVNext.Test1"
                };

            context.EnabledFeatures = hardcoded.Union(context.EnabledFeatures ?? Enumerable.Empty<string>()).Distinct().ToList();

            var shellSettings = new ShellSettings(_shellSettings);

            if (string.IsNullOrEmpty(shellSettings.DataProvider)) {
                shellSettings.DataProvider = context.DatabaseProvider;
                shellSettings.DataConnectionString = context.DatabaseConnectionString;
                shellSettings.DataTablePrefix = context.DatabaseTablePrefix;
            }

            // TODO: Add Encryption Settings in

            var shellDescriptor = new ShellDescriptor {
                Features = context.EnabledFeatures.Select(name => new ShellFeature { Name = name })
            };

            // creating a standalone environment. 
            // in theory this environment can be used to resolve any normal components by interface, and those
            // components will exist entirely in isolation - no crossover between the safemode container currently in effect

            // must mark state as Running - otherwise standalone enviro is created "for setup"
            shellSettings.State = TenantState.Running;

            // TODO: Remove and mirror Orchard Setup
            shellSettings.RequestUrlHost = _httpContextAccessor.HttpContext.Request.Host.Value;
            shellSettings.DataProvider = "InMemory";

            _shellSettingsManager.SaveSettings(shellSettings);

            return executionId;
        }
    }
}