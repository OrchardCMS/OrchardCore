using Microsoft.AspNetCore.Mvc.ApplicationParts;
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
            IRazorViewEngineFileProviderAccessor fileProviderAccessor)
        {
            lock (_synLock)
            {
                if (_cache == null)
                {
                    _cache = new DefaultCompilerCacheProvider(
                        applicationPartManager,
                        fileProviderAccessor)
                        .Cache;
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