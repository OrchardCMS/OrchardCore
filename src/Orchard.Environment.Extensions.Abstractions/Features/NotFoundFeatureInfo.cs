using System;

namespace Orchard.Environment.Extensions.Features
{
    public class NotFoundFeatureInfo : IFeatureInfo
    {
        private string _featureId;
        private IExtensionInfo _extensionInfo;

        public NotFoundFeatureInfo(string featureId, IExtensionInfo extension)
        {
            _featureId = featureId;
            _extensionInfo = extension;
        }

        public string[] Dependencies { get; } = new string[0];
        public IExtensionInfo Extension { get { return _extensionInfo; } }
        public string Id { get { return _featureId; } }
        public string Name { get; } = null;
        public double Priority { get; }

        public bool DependencyOn(IFeatureInfo feature)
        {
            return false;
        }
    }
}
