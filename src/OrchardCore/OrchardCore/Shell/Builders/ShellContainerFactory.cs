using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Builders;

public class ShellContainerFactory : IShellContainerFactory
{
    private IFeatureInfo _applicationFeature;

    private readonly IHostEnvironment _hostingEnvironment;
    private readonly IExtensionManager _extensionManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceCollection _applicationServices;

    public ShellContainerFactory(
        IHostEnvironment hostingEnvironment,
        IExtensionManager extensionManager,
        IServiceProvider serviceProvider,
        IServiceCollection applicationServices)
    {
        _hostingEnvironment = hostingEnvironment;
        _extensionManager = extensionManager;
        _applicationServices = applicationServices;
        _serviceProvider = serviceProvider;
    }

    public async Task<IServiceProvider> CreateContainerAsync(ShellSettings settings, ShellBlueprint blueprint)
    {
        var tenantServiceCollection = _serviceProvider.CreateChildContainer(_applicationServices);

        tenantServiceCollection.AddSingleton(settings);
        tenantServiceCollection.AddSingleton(sp =>
        {
            // Resolve it lazily as it's constructed lazily
            var shellSettings = sp.GetRequiredService<ShellSettings>();
            return shellSettings.ShellConfiguration;
        });

        tenantServiceCollection.AddSingleton(blueprint.Descriptor);
        tenantServiceCollection.AddSingleton(blueprint);

        // Execute IStartup registrations

        foreach (var dependency in blueprint.Dependencies.Where(t => typeof(IStartup).IsAssignableFrom(t.Key)))
        {
            tenantServiceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IStartup), dependency.Key));
        }

        // To not trigger features loading before it is normally done by 'ShellHost',
        // init here the application feature in place of doing it in the constructor.
        EnsureApplicationFeature();

        foreach (var rawStartup in blueprint.Dependencies.Keys.Where(t => t.Name == "Startup"))
        {
            // Startup classes inheriting from IStartup are already treated
            if (typeof(IStartup).IsAssignableFrom(rawStartup))
            {
                continue;
            }

            // Ignore Startup class from main application.
            if (blueprint.Dependencies.TryGetValue(rawStartup, out var startupFeatures) && startupFeatures.Any(f => f.Id == _applicationFeature.Id))
            {
                continue;
            }

            // Create a wrapper around this method
            var configureServicesMethod = rawStartup.GetMethod(
                nameof(IStartup.ConfigureServices),
                BindingFlags.Public | BindingFlags.Instance,
                null,
                CallingConventions.Any,
                [typeof(IServiceCollection)],
                null);

            var configureMethod = rawStartup.GetMethod(
                nameof(IStartup.Configure),
                BindingFlags.Public | BindingFlags.Instance);

            var orderProperty = rawStartup.GetProperty(
                nameof(IStartup.Order),
                BindingFlags.Public | BindingFlags.Instance);

            var configureOrderProperty = rawStartup.GetProperty(
                nameof(IStartup.ConfigureOrder),
                BindingFlags.Public | BindingFlags.Instance);

            // Add the startup class to the DI so we can instantiate it with
            // valid ctor arguments
            tenantServiceCollection.AddSingleton(rawStartup);

            tenantServiceCollection.AddSingleton<IStartup>(sp =>
            {
                var startupInstance = sp.GetService(rawStartup);
                return new StartupBaseMock(startupInstance, configureServicesMethod, configureMethod, orderProperty, configureOrderProperty);
            });
        }

        // Index all service descriptors by their feature id
        var featureAwareServiceCollection = new FeatureAwareServiceCollection(tenantServiceCollection);

        var shellServiceProvider = tenantServiceCollection.BuildServiceProvider(true);
        var startups = shellServiceProvider.GetServices<IStartup>();

        // IStartup instances are ordered by module dependency with an Order of 0 by default.
        // OrderBy performs a stable sort so order is preserved among equal Order values.
        startups = startups.OrderBy(s => s.Order);

        // Let any module add custom service descriptors to the tenant.
        foreach (var startup in startups)
        {
            var feature = blueprint.Dependencies.FirstOrDefault(x => x.Key == startup.GetType()).Value?.FirstOrDefault();

            // If the startup is not coming from an extension, associate it to the application feature.
            // For instance when Startup classes are registered with Configure<Startup>() from the application.

            featureAwareServiceCollection.SetCurrentFeature(feature ?? _applicationFeature);
            startup.ConfigureServices(featureAwareServiceCollection);
        }

        await shellServiceProvider.DisposeAsync();

        // Rebuild the service provider from the updated collection.
        shellServiceProvider = tenantServiceCollection.BuildServiceProvider(true);

        var typeFeatureProvider = shellServiceProvider.GetRequiredService<ITypeFeatureProvider>();
        PopulateTypeFeatureProvider(typeFeatureProvider, featureAwareServiceCollection);

        return shellServiceProvider;
    }

    private void EnsureApplicationFeature()
    {
        if (_applicationFeature is null)
        {
            lock (this)
            {
                _applicationFeature ??= _extensionManager.GetFeatures()
                        .FirstOrDefault(f => f.Id == _hostingEnvironment.ApplicationName);
            }
        }
    }

    private void PopulateTypeFeatureProvider(ITypeFeatureProvider typeFeatureProvider, FeatureAwareServiceCollection featureAwareServiceCollection)
    {
        // Register all DIed types in ITypeFeatureProvider.
        foreach (var featureServiceCollection in featureAwareServiceCollection.FeatureCollections)
        {
            foreach (var serviceDescriptor in featureServiceCollection.Value)
            {
                var type = serviceDescriptor.GetImplementationType();

                if (type is not null)
                {
                    var feature = featureServiceCollection.Key;

                    if (feature == _applicationFeature)
                    {
                        var attribute = type.GetCustomAttributes<FeatureAttribute>(false).FirstOrDefault();

                        if (attribute is not null)
                        {
                            feature = featureServiceCollection.Key.Extension.Features
                                .FirstOrDefault(f => f.Id == attribute.FeatureName)
                                ?? feature;
                        }
                    }

                    typeFeatureProvider.TryAdd(type, feature);
                }
            }
        }
    }
}
