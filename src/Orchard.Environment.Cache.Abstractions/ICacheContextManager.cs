using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Environment.Cache
{
    /// <summary>
    /// Provides the discriminator for a specific cache context by requesting all <see cref="ICacheContextProvider"/>
    /// implementations.
    /// </summary>
    public interface ICacheContextManager
    {
        Task<IEnumerable<CacheContextEntry>> GetContextAsync(IEnumerable<string> contexts);
    }
}
