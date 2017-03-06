using System.Collections.Generic;

namespace Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy
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
                    ShapeType = Adjust(info.SubPath, info.FileName, null)
                };
            }
            else
            {
                var displayType = info.FileName.Substring(lastDot + 1);
                yield return new HarvestShapeHit
                {
                    ShapeType = Adjust(info.SubPath, info.FileName.Substring(0, lastDot), displayType),
                    DisplayType = displayType
                };
            }
        }

        static string Adjust(string subPath, string fileName, string displayType)
        {
            var leader = "";
            if (subPath.StartsWith("Views/") && subPath != "Views/Items")
            {
                leader = subPath.Substring("Views/".Length) + "_";
            }

            // canonical shape type names must not have - or . to be compatible
            // with display and shape api calls)))
            var shapeType = leader + fileName.Replace("--", "__").Replace("-", "__").Replace('.', '_');

            if (string.IsNullOrEmpty(displayType))
            {
                return shapeType.ToLowerInvariant();
            }
            var firstBreakingSeparator = shapeType.IndexOf("__");
            if (firstBreakingSeparator <= 0)
            {
                return (shapeType + "_" + displayType).ToLowerInvariant();
            }

            return (shapeType.Substring(0, firstBreakingSeparator) + "_" + displayType + shapeType.Substring(firstBreakingSeparator)).ToLowerInvariant();
        }
    }
}