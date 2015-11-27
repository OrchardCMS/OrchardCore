using Orchard.Environment.Extensions.Models;
using System.Linq;

namespace Orchard.Environment.Extensions.Features
{
    public static class FeatureManagerExtensions
    {
        public static Feature GetFeature(this IFeatureManager featureManager, string id)
        {
            var feature = featureManager.GetAvailableFeatures().FirstOrDefault(x => x.Id == id);

            if (feature == null)
            {
                return null;
            }

            return new Feature
            {
                Descriptor = feature
            };
        }
    }
}