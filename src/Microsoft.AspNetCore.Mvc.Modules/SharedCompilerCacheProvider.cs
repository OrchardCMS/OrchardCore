using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    /// <summary>
    /// This implementation of <see cref="ICompilerCacheProvider"/> shares the same <see cref="ICompilerCache"/>
    /// instance across all tenants of the same application in order for the compiled view. Otherwise each
    /// tenant would get its own compiled view.
    /// </summary>
    public class SharedCompilerCacheProvider : ICompilerCacheProvider
    {
        private static ICompilerCache _cache;
        private static object _synLock = new object();

        public SharedCompilerCacheProvider(
            ApplicationPartManager applicationPartManager,
            IRazorViewEngineFileProviderAccessor fileProviderAccessor,
            IHostingEnvironment env)
        {
            lock (_synLock)
            {
                if (_cache == null)
                {
                    var feature = new ViewsFeature();

                    // Applying ViewsFeatureProvider to gather any precompiled view
                    new ViewsFeatureProvider().PopulateFeature(
                        new AssemblyPart[]
                        {
                            new AssemblyPart(Assembly.Load(new AssemblyName(env.ApplicationName)))
                        },
                        feature);

                    _cache = new CompilerCache(fileProviderAccessor.FileProvider, feature.Views);
                }
            }
        }

        /// <inheritdoc />
        public ICompilerCache Cache
        {
            get
            {
                return _cache;
            }
        }
    }
}
