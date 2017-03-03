using System.Collections.Generic;
using Microsoft.AspNetCore.Nancy.Modules.AssemblyCatalogs;
using Nancy;

namespace Microsoft.AspNetCore.Nancy.Modules
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
