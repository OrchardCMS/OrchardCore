using Orchard.Environment.Extensions.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions
{
    public class CompositeExtensionOrderingStrategy : IExtensionOrderingStrategy
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

        public double Priority { get { throw new NotSupportedException(); } }

        public int Compare(IFeatureInfo observer, IFeatureInfo subject)
        {
            foreach (var strategy in _extensionOrderingStrategies.OrderByDescending(x => x.Priority))
            {
                var compare = strategy.Compare(observer, subject);
                if (compare != 0)
                {
                    return compare;
                }
            }
            return 0;
        }
    }
}
