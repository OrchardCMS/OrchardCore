using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nancy;

namespace Microsoft.AspNetCore.Nancy.Modules
{
    public class AmbientAssemblyCatalog : IAssemblyCatalog
    {
        private readonly IEnumerable<Assembly> _assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContextAssemblyCatalog"/> class.
        /// </summary>
        public AmbientAssemblyCatalog(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies;
        }

        /// <summary>
        /// Gets all <see cref="Assembly"/> instances in the catalog.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Assembly"/> instances.</returns>
        public IReadOnlyCollection<Assembly> GetAssemblies()
        {
            return _assemblies.ToArray();
        }
    }
}
