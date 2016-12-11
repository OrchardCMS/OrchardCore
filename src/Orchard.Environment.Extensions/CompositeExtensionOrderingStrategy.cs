using Orchard.Environment.Extensions.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions
{
    internal class CompositeExtensionOrderingStrategy : IExtensionOrderingStrategy
    {
        private readonly IExtensionOrderingStrategy[] _extensionOrderingStrategies;
        public CompositeExtensionOrderingStrategy(params IExtensionOrderingStrategy[] extensionOrderingStrategies)
        {
            _extensionOrderingStrategies = extensionOrderingStrategies ?? new IExtensionOrderingStrategy[0];
        }

        public CompositeExtensionOrderingStrategy(IEnumerable<IExtensionOrderingStrategy> extensionOrderingStrategies)
        {
            if (extensionOrderingStrategies == null)
            {
                throw new ArgumentNullException(nameof(extensionOrderingStrategies));
            }
            _extensionOrderingStrategies = extensionOrderingStrategies.ToArray();
        }

        public bool HasDependency(IFeatureInfo observer, IFeatureInfo subject)
        {
            return _extensionOrderingStrategies
                .Any(s => s.HasDependency(observer, subject));
        }
    }
}
