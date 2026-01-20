using System;
using System.Linq;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Modules.Manifest;

namespace OrchardCore.DisplayManagement.Events
{
    public class ThemeFeatureBuilderEvents : FeatureBuilderEvents
    {
        public override void Building(FeatureBuildingContext context)
        {
            var moduleInfo = context.ExtensionInfo.Manifest.ModuleInfo;

            if (moduleInfo is ThemeAttribute || (moduleInfo is ModuleMarkerAttribute &&
                moduleInfo.Type.Equals("Theme", StringComparison.OrdinalIgnoreCase)))
            {
                var extensionInfo = new ThemeExtensionInfo(context.ExtensionInfo);

                if (extensionInfo.HasBaseTheme())
                {
                    context.FeatureDependencyIds = context
                        .FeatureDependencyIds
                        .Concat(new[] { extensionInfo.BaseTheme })
                        .ToArray();
                }

                context.ExtensionInfo = extensionInfo;
            }
        }
    }
}
