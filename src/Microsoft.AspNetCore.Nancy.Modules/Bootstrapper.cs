using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Configuration;
using Nancy.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell.Builders;
using Nancy.TinyIoc;
using System.Reflection;

namespace Microsoft.AspNetCore.Nancy.Modules
{
    public class NancyAspNetCoreBootstrapper : DefaultNancyBootstrapper
    {
        private readonly IAssemblyCatalog _assemblyCatalog;

        public NancyAspNetCoreBootstrapper(
            IEnumerable<IAssemblyCatalog> assemblyCatalogs)
        {
            _assemblyCatalog = new CompositeAssemblyCatalog(assemblyCatalogs);
        }

        protected override IAssemblyCatalog AssemblyCatalog
        {
            get
            {
                return _assemblyCatalog;
            }
        }
    }

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
