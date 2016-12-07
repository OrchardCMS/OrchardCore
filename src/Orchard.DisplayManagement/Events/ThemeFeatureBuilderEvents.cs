using Orchard.Environment.Extensions.Features;
using System.Linq;

namespace Orchard.DisplayManagement.Events
{
    public class ThemeFeatureBuilderEvents : FeatureBuilderEvents
    {
        public override void Building(FeatureBuildingContext context)
        {
            var baseTheme = context.ExtensionInfo.Manifest.ConfigurationRoot["basetheme"];

            if (baseTheme != null && baseTheme.Length != 0)
            {
                if (!context.FeatureDependencyIds.Contains(baseTheme))
                {
                    var temp = context.FeatureDependencyIds.ToList();
                    temp.Add(baseTheme);
                    context.FeatureDependencyIds = temp.ToArray();
                }
            }
        }
    }
}
