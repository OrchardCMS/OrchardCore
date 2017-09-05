using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nancy;

namespace OrchardCore.Nancy.AssemblyCatalogs
{
    public class CompositeAssemblyCatalog : IAssemblyCatalog
    {
        private readonly IAssemblyCatalog[] _assemblyCatalogs;
        public CompositeAssemblyCatalog(params IAssemblyCatalog[] assemblyCatalogs)
        {
            _assemblyCatalogs = assemblyCatalogs ?? new IAssemblyCatalog[0];
        }

        public CompositeAssemblyCatalog(IEnumerable<IAssemblyCatalog> assemblyCatalogs)
        {
            if (assemblyCatalogs == null)
            {
                throw new ArgumentNullException(nameof(assemblyCatalogs));
            }
            _assemblyCatalogs = assemblyCatalogs.ToArray();
        }

        public IReadOnlyCollection<Assembly> GetAssemblies()
        {
            List<Assembly> featureInfos =
                new List<Assembly>();

            foreach (var provider in _assemblyCatalogs)
            {
                featureInfos.AddRange(provider.GetAssemblies());
            }

            return featureInfos;
        }
    }
}
