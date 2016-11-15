using Orchard.Environment.Extensions.Utility;
using Orchard.Environment.Extensions.Features;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Orchard.Environment.Extensions
{
    public class ExtensionInfoList : IExtensionInfoList
    {
        private readonly IDictionary<string, IExtensionInfo> _extensionsByKey;

        public ExtensionInfoList(IDictionary<string, IExtensionInfo> extensions) {
            _extensionsByKey = extensions;
        }

        public IExtensionInfo this[string key]
        {
            get { return _extensionsByKey[key]; }
        }

        public IExtensionInfo this[int index]
        {
            get { return _extensionsByKey.Values.ToList()[index]; }
        }

        public int Count
        {
            get { return _extensionsByKey.Count; }
        }

        private IFeatureInfoList _features;

        public IFeatureInfoList Features {
            get
            {
                if (_features == null)
                {
                    _features = new FeatureInfoList(
                        _extensionsByKey
                            .Values
                            .SelectMany(x => x.Features)
                            .OrderByDependenciesAndPriorities(HasDependency, GetPriority)
                            .ToDictionary(x => x.Id, y => y));
                }

                return _features;
            }
        }

        /// <summary>
        /// Returns true if the item has an explicit or implicit dependency on the subject
        /// </summary>
        /// <param name="item"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        private static bool HasDependency(IFeatureInfo item, IFeatureInfo subject)
        {
            return item.DependencyOn(subject);
        }

        private static double GetPriority(IFeatureInfo featureInfo)
        {
            return featureInfo.Priority;
        }

        public IEnumerator<IExtensionInfo> GetEnumerator()
        {
            return _extensionsByKey.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _extensionsByKey.Values.GetEnumerator();
        }
    }
}
