using Orchard.Environment.Extensions.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Environment.Extensions.Features
{
    public static class FeatureManagerExtensions
    {
        public static async Task<Feature> GetFeatureAsync(this IFeatureManager featureManager, string id)
        {
            var feature = (await featureManager.GetAvailableFeaturesAsync()).FirstOrDefault(x => x.Id == id);

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