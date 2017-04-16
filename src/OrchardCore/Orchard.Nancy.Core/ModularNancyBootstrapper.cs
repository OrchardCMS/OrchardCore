using System.Collections.Generic;
using Orchard.Nancy.AssemblyCatalogs;
using Nancy;

namespace Orchard.Nancy
{
    public class ModularNancyBootstrapper : DefaultNancyBootstrapper
    {
        private readonly IAssemblyCatalog _assemblyCatalog;

        public ModularNancyBootstrapper(
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
