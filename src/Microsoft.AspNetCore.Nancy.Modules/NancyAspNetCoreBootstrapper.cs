using System.Collections.Generic;
using Nancy;

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
}
