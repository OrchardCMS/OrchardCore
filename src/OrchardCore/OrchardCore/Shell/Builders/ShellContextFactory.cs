using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell.Builders
{
    public class ShellContextFactory : IShellContextFactory
    {
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly IEnumerable<ShellFeature> _shellFeatures;
        private readonly ILogger _logger;

        public ShellContextFactory(
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory,
            IEnumerable<ShellFeature> shellFeatures,
            ILogger<ShellContextFactory> logger)
        {
            _compositionStrategy = compositionStrategy;
            _shellContainerFactory = shellContainerFactory;
            _shellFeatures = shellFeatures;
            _logger = logger;
        }

        async Task<ShellContext> IShellContextFactory.CreateShellContextAsync(ShellSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Creating shell context for tenant '{TenantName}'", settings.Name);
            }

            var describedContext = await CreateDescribedContextAsync(settings, new ShellDescriptor());

            ShellDescriptor currentDescriptor = null;
            await (await describedContext.CreateScopeAsync()).UsingServiceScopeAsync(async scope =>
            {
                var shellDescriptorManager = scope.ServiceProvider.GetService<IShellDescriptorManager>();
                currentDescriptor = await shellDescriptorManager.GetShellDescriptorAsync();
            });

            if (currentDescriptor is not null)
            {
                await describedContext.DisposeAsync();
                return await CreateDescribedContextAsync(settings, currentDescriptor);
            }

            return describedContext;
        }

        Task<ShellContext> IShellContextFactory.CreateSetupContextAsync(ShellSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("No shell settings available. Creating shell context for setup");
            }

            var descriptor = MinimumShellDescriptor();

            return CreateDescribedContextAsync(settings, descriptor);
        }

        public async Task<ShellContext> CreateDescribedContextAsync(ShellSettings settings, ShellDescriptor shellDescriptor)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Creating described context for tenant '{TenantName}'", settings.Name);
            }

            await settings.EnsureConfigurationAsync();

            var blueprint = await _compositionStrategy.ComposeAsync(settings, shellDescriptor);
            var provider = await _shellContainerFactory.CreateContainerAsync(settings, blueprint);

            var options = provider.GetService<IOptions<ShellContainerOptions>>().Value;
            foreach (var initializeAsync in options.Initializers)
            {
                await initializeAsync(provider);
            }

            return new ShellContext
            {
                Settings = settings,
                Blueprint = blueprint,
                ServiceProvider = provider
            };
        }

        /// <summary>
        /// The minimum shell descriptor is used to bootstrap the first container that will be used
        /// to call all module IStartup implementation. It's composed of module names that reference
        /// core components necessary for the desired scenario.
        /// </summary>
        /// <returns></returns>
        private ShellDescriptor MinimumShellDescriptor()
        {
            // Load default features from the list of registered ShellFeature instances in the DI

            return new ShellDescriptor
            {
                SerialNumber = -1,
                Features = new List<ShellFeature>(_shellFeatures)
            };
        }
    }
}
