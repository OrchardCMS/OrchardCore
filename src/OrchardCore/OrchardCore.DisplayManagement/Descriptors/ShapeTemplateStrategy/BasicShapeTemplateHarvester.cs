using System;
using System.Collections.Generic;

namespace OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy
{
    public class BasicShapeTemplateHarvester : IShapeTemplateHarvester
    {
        public IEnumerable<string> SubPaths()
        {
            return new[] { "Views", "Views/Items", "Views/Parts", "Views/Fields", "Views/Elements" };
        }

        public IEnumerable<HarvestShapeHit> HarvestShape(HarvestShapeInfo info)
        {
            var lastDash = info.FileName.LastIndexOf('-');
            var lastDot = info.FileName.LastIndexOf('.');
            if (lastDot <= 0 || lastDot < lastDash)
            {
                yield return new HarvestShapeHit
                {
                    ShapeType = Adjust(info.SubPath, info.FileName, null),
                };
            }
            else
            {
                var displayType = info.FileName[(lastDot + 1)..];
                yield return new HarvestShapeHit
                {
                    ShapeType = Adjust(info.SubPath, info.FileName[..lastDot], displayType),
                    DisplayType = displayType,
                };
            }
        }

        private static string Adjust(string subPath, string fileName, string displayType)
        {
            var leader = "";
            if (subPath.StartsWith("Views/", StringComparison.Ordinal) && subPath != "Views/Items")
            {
                leader = String.Concat(subPath.AsSpan("Views/".Length), "_");
            }

            // canonical shape type names must not have - or . to be compatible
            // with display and shape api calls)))
            var shapeType = leader + fileName.Replace("--", "__").Replace("-", "__").Replace('.', '_');

            if (String.IsNullOrEmpty(displayType))
            {
                return shapeType.ToLowerInvariant();
            }

            var firstBreakingSeparator = shapeType.IndexOf("__", StringComparison.Ordinal);
            if (firstBreakingSeparator <= 0)
            {
                return (shapeType + "_" + displayType).ToLowerInvariant();
            }

            return String.Concat(
                shapeType.AsSpan(0, firstBreakingSeparator), "_", displayType, shapeType.AsSpan(firstBreakingSeparator))
                .ToLowerInvariant();
        }
    }
}
