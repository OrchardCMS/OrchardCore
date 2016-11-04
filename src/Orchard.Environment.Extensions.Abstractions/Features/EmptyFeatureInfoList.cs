using System.Collections;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Features
{
    public class EmptyFeatureInfoList : IFeatureInfoList
    {
        private readonly IDictionary<string, IFeatureInfo> _emptyDictionary =
            new Dictionary<string, IFeatureInfo>();

        private readonly IList<IFeatureInfo> _emptyList =
            new List<IFeatureInfo>();

        public IFeatureInfo this[int index]
        {
            get
            {
                return _emptyList[index];
            }
        }

        public IFeatureInfo this[string key]
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

        public IExtensionInfoList Extensions
        {
            get
            {
                return new EmptyExtensionInfoList();
            }
        }

        public IEnumerator<IFeatureInfo> GetEnumerator()
        {
            return _emptyList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _emptyList.GetEnumerator();
        }
    }
}
