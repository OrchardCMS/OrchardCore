using Orchard.Environment.Extensions.Features;
using System.Collections.Generic;
using System.Collections;

namespace Orchard.Environment.Extensions
{
    public class EmptyExtensionInfoList : IExtensionInfoList
    {
        private readonly IDictionary<string, IExtensionInfo> _emptyDictionary =
            new Dictionary<string, IExtensionInfo>();

        private readonly IList<IExtensionInfo> _emptyList =
            new List<IExtensionInfo>();

        public IExtensionInfo this[int index]
        {
            get
            {
                return _emptyList[index];
            }
        }

        public IExtensionInfo this[string key]
        {
            get
            {
                return _emptyDictionary[key];
            }
        }

        public int Count
        {
            get
            {
                return 0;
            }
        }

        public IFeatureInfoList Features
        {
            get
            {
                return new EmptyFeatureInfoList();
            }
        }

        public IEnumerator<IExtensionInfo> GetEnumerator()
        {
            return _emptyList.GetEnumerator();
        }

        public bool HasFeature(string featureId)
        {
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _emptyList.GetEnumerator();
        }
    }
}
