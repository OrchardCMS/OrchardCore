using Orchard.DependencyInjection;
using System.Collections.Generic;

namespace Orchard.Environment.Cache.Abstractions
{
    /// <summary>
    /// Returns a set of values describing the discriminators of the context and a value 
    /// which changes whenever the state of the discriminator changes, for instance a serial
    /// number of a timestamp.
    /// </summary>
    public interface ICacheContextProvider : IDependency
    {
        void PopulateContextEntries(IEnumerable<string> contexts, List<CacheContextEntry> entries);
    }
}
