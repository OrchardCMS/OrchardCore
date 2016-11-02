using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Orchard.Environment.Extensions.Features
{
    public class FeatureInfoList : IFeatureInfoList
    {
        private readonly IDictionary<string, IFeatureInfo> _featuresByKey;
        private readonly IReadOnlyList<IFeatureInfo> _features;

        public FeatureInfoList(IDictionary<string, IFeatureInfo> features) {
            _featuresByKey = features;
            _features = features.Select(e => e.Value).ToList();
        }

        public IFeatureInfo this[string key]
        {
            get { return _featuresByKey[key]; }
        }

        public IFeatureInfo this[int index]
        {
            get { return _features[index]; }
        }

        public int Count
        {
            get { return _featuresByKey.Count; }
        }

        public IEnumerable<IExtensionInfo> Extensions {
            get
            {
                return _features.Select(x => x.Extension);
            }
        }

        public IEnumerator<IFeatureInfo> GetEnumerator()
        {
            return _features.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _features.GetEnumerator();
        }
    }
}
