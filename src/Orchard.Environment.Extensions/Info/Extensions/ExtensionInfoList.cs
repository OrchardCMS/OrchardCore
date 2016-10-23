using Orchard.Environment.Extensions.Info.Extensions.Utility;
using Orchard.Environment.Extensions.Info.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Orchard.Environment.Extensions.Info.Extensions
{
    public class ExtensionInfoList : IExtensionInfoList
    {
        private IDictionary<string, IExtensionInfo> _extensionsByKey;
        private IReadOnlyList<IExtensionInfo> _extensions;

        public ExtensionInfoList(IDictionary<string, IExtensionInfo> extensions) {
            _extensionsByKey = extensions;
            _extensions = extensions.Select(e => e.Value).ToList();
        }

        public IExtensionInfo this[string key]
        {
            get { return _extensionsByKey[key]; }
        }

        public IExtensionInfo this[int index]
        {
            get { return _extensions[index]; }
        }

        public int Count
        {
            get { return _extensions.Count; }
        }

        public IEnumerable<IFeatureInfo> GetAllFeatures() {
            return _extensions.SelectMany(x => x.Features)
                .OrderByDependenciesAndPriorities(HasDependency, GetPriority);
        }

        /// <summary>
        /// Returns true if the item has an explicit or implicit dependency on the subject
        /// </summary>
        /// <param name="item"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static bool HasDependency(IFeatureInfo item, IFeatureInfo subject)
        {
            return item.Dependencies.Any(x => 
                StringComparer.OrdinalIgnoreCase.Equals(x, subject.Id));
        }

        internal static double GetPriority(IFeatureInfo featureInfo)
        {
            return featureInfo.Priority;
        }

        public IEnumerator<IExtensionInfo> GetEnumerator()
        {
            return _extensions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _extensions.GetEnumerator();
        }
    }
}
