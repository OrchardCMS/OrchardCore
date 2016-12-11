using Orchard.DisplayManagement.Extensions;
using Orchard.Environment.Extensions.Features;
using System.Linq;

namespace Orchard.DisplayManagement.Events
{
    public class ThemeFeatureBuilderEvents : FeatureBuilderEvents
    {
        public override void Building(FeatureBuildingContext context)
        {
            if (context.ExtensionInfo.Manifest.IsTheme())
            {
                ThemeExtensionInfo extensionInfo = new ThemeExtensionInfo(context.ExtensionInfo);

                if (extensionInfo.HasBaseTheme())
                {
                    if (!context.FeatureDependencyIds.Contains(extensionInfo.BaseTheme))
                    {
                        var temp = context.FeatureDependencyIds.ToList();
                        temp.Add(extensionInfo.BaseTheme);
                        context.FeatureDependencyIds = temp.ToArray();
                    }
                }

                context.ExtensionInfo = extensionInfo;
            }
        }
    }
}
