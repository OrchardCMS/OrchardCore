using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.Builders.Models;

namespace OrchardCore.Environment.Shell.Builders
{
    public class ShellContainerFactory : IShellContainerFactory
    {
        private readonly IFeatureInfo _applicationFeature;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IServiceCollection _applicationServices;

        public ShellContainerFactory(
            IHostingEnvironment hostingEnvironment,
            IExtensionManager extensionManager,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            ILogger<ShellContainerFactory> logger,
            IServiceCollection applicationServices)
        {
            _applicationFeature = extensionManager.GetFeatures().FirstOrDefault(
                f => f.Id == hostingEnvironment.ApplicationName);

            _applicationServices = applicationServices;
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public void AddCoreServices(IServiceCollection services)
        {
            services.TryAddScoped<IShellStateUpdater, ShellStateUpdater>();
            services.TryAddScoped<IShellStateManager, NullShellStateManager>();
            services.AddScoped<IShellDescriptorManagerEventHandler, ShellStateCoordinator>();
        }

        public IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint)
        {
            var tenantServiceCollection = _serviceProvider.CreateChildContainer(_applicationServices);

            tenantServiceCollection.AddSingleton(settings);
            tenantServiceCollection.AddSingleton(blueprint.Descriptor);
            tenantServiceCollection.AddSingleton(blueprint);

            AddCoreServices(tenantServiceCollection);

            // Execute IStartup registrations

            // TODO: Use StartupLoader in RTM and then don't need to register the classes anymore then

            var moduleServiceCollection = _serviceProvider.CreateChildContainer(_applicationServices);

            foreach (var dependency in blueprint.Dependencies.Where(t => typeof(Modules.IStartup).IsAssignableFrom(t.Key)))
            {
                moduleServiceCollection.AddSingleton(typeof(Modules.IStartup), dependency.Key);
                tenantServiceCollection.AddSingleton(typeof(Modules.IStartup), dependency.Key);
            }

            // Add a default configuration if none has been provided
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            moduleServiceCollection.TryAddSingleton(configuration);
            tenantServiceCollection.TryAddSingleton(configuration);

            // Make shell settings available to the modules
            moduleServiceCollection.AddSingleton(settings);

            var moduleServiceProvider = moduleServiceCollection.BuildServiceProvider(true);

            // Index all service descriptors by their feature id
            var featureAwareServiceCollection = new FeatureAwareServiceCollection(tenantServiceCollection);

            var startups = moduleServiceProvider.GetServices<Modules.IStartup>();

            // IStartup instances are ordered by module dependency with an Order of 0 by default.
            // OrderBy performs a stable sort so order is preserved among equal Order values.
            startups = startups.OrderBy(s => s.Order);

            // Let any module add custom service descriptors to the tenant
            foreach (var startup in startups)
            {
                var feature = blueprint.Dependencies.FirstOrDefault(x => x.Key == startup.GetType()).Value?.FeatureInfo;

                // If the startup is not coming from an extension, associate it to the application feature.
                // For instance when Startup classes are registered with Configure<Startup>() from the application.

                featureAwareServiceCollection.SetCurrentFeature(feature ?? _applicationFeature);
                startup.ConfigureServices(featureAwareServiceCollection);
            }

            (moduleServiceProvider as IDisposable).Dispose();
            
            var shellServiceProvider = tenantServiceCollection.BuildServiceProvider(true);

            // Register all DIed types in ITypeFeatureProvider
            var typeFeatureProvider = shellServiceProvider.GetRequiredService<ITypeFeatureProvider>();

            foreach (var featureServiceCollection in featureAwareServiceCollection.FeatureCollections)
            {
                foreach (var serviceDescriptor in featureServiceCollection.Value)
                {
                    if (serviceDescriptor.ImplementationType != null)
                    {
                        typeFeatureProvider.TryAdd(serviceDescriptor.ImplementationType, featureServiceCollection.Key);
                    }
                    else if (serviceDescriptor.ImplementationInstance != null)
                    {
                        typeFeatureProvider.TryAdd(serviceDescriptor.ImplementationInstance.GetType(), featureServiceCollection.Key);
                    }
                    else
                    {
                        // Factory, we can't know which type will be returned
                    }
                }
            }

            return shellServiceProvider;
        }
    }
}