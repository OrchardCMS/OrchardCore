using System;

namespace Orchard.Environment.Extensions.Features
{
    public class NotFoundFeatureInfo : IFeatureInfo
    {
        public string[] Dependencies { get; } = new string[0];
        public IExtensionInfo Extension { get; } = new NotFoundExtensionInfo();
        public string Id { get; } = null;
        public string Name { get; } = null;
        public double Priority { get; }

        public bool DependencyOn(IFeatureInfo feature)
        {
            return false;
        }
    }
}
