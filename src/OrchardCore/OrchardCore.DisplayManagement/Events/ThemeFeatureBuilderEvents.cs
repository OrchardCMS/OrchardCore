using System.Linq;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.DisplayManagement.Events
{
    public class ThemeFeatureBuilderEvents : FeatureBuilderEvents
    {
        public override void Building(FeatureBuildingContext context)
        {
            if (context.ExtensionInfo.Manifest.ModuleInfo is ThemeAttribute)
            {
                var extensionInfo = new ThemeExtensionInfo(context.ExtensionInfo);

                if (extensionInfo.HasBaseTheme())
                {
                    context.FeatureDependencyIds = context
                        .FeatureDependencyIds
                        .Concat(new [] { extensionInfo.BaseTheme })
                        .ToArray();
                }

                context.ExtensionInfo = extensionInfo;
            }
        }
    }
}
