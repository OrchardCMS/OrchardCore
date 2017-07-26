using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace Orchard.Mvc
{
    /// <summary>
    /// This implementation of <see cref="ICompilerCacheProvider"/> shares the same <see cref="ICompilerCache"/>
    /// instance across all tenants of the same application in order for the compiled views. Otherwise each
    /// tenant would get its own compiled views.
    /// </summary>
    public class SharedCompilerCacheProvider : ICompilerCacheProvider
    {
        private static IDictionary<string, Type> _views;
        private static object _synLock = new object();
        private readonly CompilerCache _cache;

        public SharedCompilerCacheProvider(
            ApplicationPartManager applicationPartManager,
            IRazorViewEngineFileProviderAccessor fileProviderAccessor,
            IEnumerable<IApplicationFeatureProvider<ViewsFeature>> viewsFeatureProviders,
            IHostingEnvironment env)
        {
            if (_views == null)
            {
                lock (_synLock)
                {
                    if (_views == null)
                    {
                        var feature = new ViewsFeature();

                        var featureProviders = applicationPartManager.FeatureProviders
                            .OfType<IApplicationFeatureProvider<ViewsFeature>>()
                            .ToList();

                        featureProviders.AddRange(viewsFeatureProviders);

                        var assemblyParts = new AssemblyPart[]
                        {
                            new AssemblyPart(Assembly.Load(new AssemblyName(env.ApplicationName)))
                        };

                        foreach (var provider in featureProviders)
                        {
                            provider.PopulateFeature(assemblyParts, feature);
                        }

                        _views = feature.Views;
                    }
                }
            }

            _cache = new CompilerCache(fileProviderAccessor.FileProvider, _views);
        }

        /// <inheritdoc />
        public ICompilerCache Cache
        {
            get { return _cache; }
        }
    }
}
