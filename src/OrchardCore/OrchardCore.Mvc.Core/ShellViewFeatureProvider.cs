using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;

namespace OrchardCore.Mvc;

public class ShellViewFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
{
    private readonly IHostEnvironment _hostingEnvironment;
    private readonly IApplicationContext _applicationContext;

    private ApplicationPartManager _applicationPartManager;
    private IEnumerable<IApplicationFeatureProvider<ViewsFeature>> _featureProviders;

    public ShellViewFeatureProvider(IServiceProvider services)
    {
        _hostingEnvironment = services.GetRequiredService<IHostEnvironment>();
        _applicationContext = services.GetRequiredService<IApplicationContext>();
    }

    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
    {
        EnsureScopedServices();

        // Check if the feature can be retrieved from the shell scope.
        var viewsFeature = ShellScope.GetFeature<ViewsFeature>();
        if (viewsFeature is not null)
        {
            foreach (var descriptor in viewsFeature.ViewDescriptors)
            {
                feature.ViewDescriptors.Add(descriptor);
            }

            return;
        }

        // Set it as a shell scope feature to be used later on.
        ShellScope.SetFeature(feature);

        PopulateFeatureInternal(feature);

        // Apply views feature providers registered at the tenant level.
        foreach (var provider in _featureProviders)
        {
            provider.PopulateFeature(parts, feature);
        }
    }

    private void PopulateFeatureInternal(ViewsFeature feature)
    {
        // Retrieve mvc views feature providers but not this one.
        var mvcFeatureProviders = _applicationPartManager.FeatureProviders
            .OfType<IApplicationFeatureProvider<ViewsFeature>>()
            .Where(p => p.GetType() != typeof(ShellViewFeatureProvider));

        var modules = _applicationContext.Application.Modules;
        var moduleFeature = new ViewsFeature();

        foreach (var module in modules)
        {

            var assembliesWithViews = new List<Assembly>();

            var relatedAssemblyAttribute = module.Assembly.GetCustomAttribute<RelatedAssemblyAttribute>();

            // Is there a dedicated Views assembly (< net6.0)
            if (relatedAssemblyAttribute != null)
            {
                var precompiledAssemblyPath = Path.Combine(Path.GetDirectoryName(module.Assembly.Location), relatedAssemblyAttribute.AssemblyFileName + ".dll");

                if (File.Exists(precompiledAssemblyPath))
                {
                    try
                    {
                        var assembly = Assembly.LoadFile(precompiledAssemblyPath);
                        assembliesWithViews.Add(assembly);
                    }
                    catch (FileLoadException)
                    {
                        // Don't throw if assembly cannot be loaded. This can happen if the file is not a managed assembly.
                    }
                }
            }

            // Look for compiled views in the same assembly as the module.
            if (module.Assembly.GetCustomAttributes<RazorCompiledItemAttribute>().Any())
            {
                assembliesWithViews.Add(module.Assembly);
            }

            foreach (var assembly in assembliesWithViews)
            {
                var applicationPart = new ApplicationPart[] { new TenantCompiledRazorAssemblyPart(assembly) };

                foreach (var provider in mvcFeatureProviders)
                {
                    provider.PopulateFeature(applicationPart, moduleFeature);
                }

                // Razor views are precompiled in the context of their modules, but at runtime
                // their paths need to be relative to the virtual "Areas/{ModuleId}" folders.
                // Note: For the app's module this folder is "Areas/{env.ApplicationName}".
                foreach (var descriptor in moduleFeature.ViewDescriptors)
                {
                    descriptor.RelativePath = '/' + module.SubPath + descriptor.RelativePath;
                    feature.ViewDescriptors.Add(descriptor);
                }

                // For the app's module we still allow to explicitly specify view paths relative to the app content root.
                // So for the application's module we re-apply the feature providers without updating the relative paths.
                // Note: This is only needed in prod mode if app's views are precompiled and views files no longer exist.
                if (module.Name == _hostingEnvironment.ApplicationName)
                {
                    foreach (var provider in mvcFeatureProviders)
                    {
                        provider.PopulateFeature(applicationPart, moduleFeature);
                    }

                    foreach (var descriptor in moduleFeature.ViewDescriptors)
                    {
                        feature.ViewDescriptors.Add(descriptor);
                    }
                }

                moduleFeature.ViewDescriptors.Clear();
            }
        }
    }

    private void EnsureScopedServices()
    {
        var services = ShellScope.Services;

        // The scope is null when this code is called through a 'ChangeToken' callback, e.g to recompile razor pages.
        // So, here we resolve and cache tenant level singletons, application singletons can be resolved in the ctor.

        if (services != null && _featureProviders == null)
        {
            lock (this)
            {
                if (_featureProviders == null)
                {
                    _applicationPartManager = services.GetRequiredService<ApplicationPartManager>();
                    _featureProviders = services.GetServices<IApplicationFeatureProvider<ViewsFeature>>();
                }
            }
        }
    }
}
