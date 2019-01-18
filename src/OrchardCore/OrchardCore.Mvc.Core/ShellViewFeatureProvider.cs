using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Mvc
{
    public class ShellViewFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IApplicationContext _applicationContext;

        private ApplicationPartManager _applicationPartManager;
        private IEnumerable<IApplicationFeatureProvider<ViewsFeature>> _featureProviders;

        public ShellViewFeatureProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            var services = _httpContextAccessor.HttpContext.RequestServices;
            _hostingEnvironment = services.GetRequiredService<IHostingEnvironment>();
            _applicationContext = services.GetRequiredService<IApplicationContext>();
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            var services = _httpContextAccessor.HttpContext?.RequestServices;

            // 'HttpContext' is null when this code is called through a 'ChangeToken' callback, e.g to recompile razor pages.
            // So, here we resolve and cache tenant level singletons, application singletons are resolved in the constructor.

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

            // Module compiled views are not served while in dev.
            if (!_hostingEnvironment.IsDevelopment())
            {
                // Retrieve mvc views feature providers but not this one.
                var mvcFeatureProviders = _applicationPartManager.FeatureProviders
                    .OfType<IApplicationFeatureProvider<ViewsFeature>>()
                    .Where(p => p.GetType() != typeof(ShellViewFeatureProvider));

                var modules = _applicationContext.Application.Modules;
                var moduleFeature = new ViewsFeature();

                foreach (var module in modules)
                {
                    var precompiledAssemblyPath = Path.Combine(Path.GetDirectoryName(module.Assembly.Location),
                        module.Assembly.GetName().Name + ".Views.dll");

                    if (File.Exists(precompiledAssemblyPath))
                    {
                        try
                        {
                            var assembly = Assembly.LoadFile(precompiledAssemblyPath);

                            foreach (var provider in mvcFeatureProviders)
                            {
                                provider.PopulateFeature(new ApplicationPart[] { new CompiledRazorAssemblyPart(assembly) }, moduleFeature);
                            }

                            // Razor views are precompiled in the context of their modules, but at runtime
                            // their paths need to be relative to the virtual "Areas/{ModuleId}" folders.
                            foreach (var descriptor in moduleFeature.ViewDescriptors)
                            {
                                descriptor.RelativePath = '/' + module.SubPath + descriptor.RelativePath;
                                feature.ViewDescriptors.Add(descriptor);
                            }

                            moduleFeature.ViewDescriptors.Clear();
                        }
                        catch (FileLoadException)
                        {
                            // Don't throw if assembly cannot be loaded. This can happen if the file is not a managed assembly.
                        }
                    }
                }
            }

            // Apply views feature providers registered at the tenant level.
            foreach (var provider in _featureProviders)
            {
                provider.PopulateFeature(parts, feature);
            }
        }
    }
}