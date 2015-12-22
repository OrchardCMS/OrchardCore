using Orchard.DependencyInjection;
using System.Collections.Generic;

namespace Orchard.Environment.Cache.Abstractions
{
    /// <summary>
    /// Provides the discriminator for a specific cache context by requesting all <see cref="ICacheContextProvider"/>
    /// implementations.
    /// </summary>
    public interface ICacheContextManager : IDependency
    {
        IEnumerable<CacheContextEntry> GetContext(IEnumerable<string> contexts);
    }
}
