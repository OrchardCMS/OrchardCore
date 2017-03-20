using System.Collections.Generic;
using OrchardCore.Nancy.Modules.AssemblyCatalogs;
using Nancy;

namespace OrchardCore.Nancy.Modules
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
