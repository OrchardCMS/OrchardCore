using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions.Features
{
    public class FeatureInfoList : IFeatureInfoList
    {
        private readonly IList<IFeatureInfo> _features;

        public FeatureInfoList(IList<IFeatureInfo> features) {
            _features = features;
        }

        public IFeatureInfo this[string key]
        {
            get { return _features.First(x => x.Id == key); }
        }

        private IExtensionInfoList _extensions;

        public IExtensionInfoList Extensions
        {
            get
            {
                if (_extensions == null)
                {
                    _extensions = new ExtensionInfoList(
                        _features.Select(x => x.Extension).ToList());
                }

                return _extensions;
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
