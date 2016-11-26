using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions.Utility;

namespace Orchard.Environment.Extensions.Features
{
    public class FeatureInfoList : IFeatureInfoList
    {
        private readonly IList<IFeatureInfo> _features;

        public FeatureInfoList(IList<IFeatureInfo> features)
        {
            _features = features
                .OrderByDependenciesAndPriorities(HasDependency, GetPriority)
                .ToList();
        }

        public IFeatureInfo this[string key]
        {
            get { return _features.First(x => x.Id == key); }
        }

        public IFeatureInfo this[int index]
        {
            get { return _features[index]; }
        }

        public int Count
        {
            get { return _features.Count; }
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
    }
}
