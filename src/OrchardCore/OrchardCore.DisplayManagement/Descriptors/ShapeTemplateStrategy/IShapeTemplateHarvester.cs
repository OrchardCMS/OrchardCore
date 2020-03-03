using System.Collections.Generic;

namespace OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy
{
    /// <summary>
    /// This service determines which paths to examine, and provides
    /// the shape type based on the template file paths discovered
    /// </summary>
    public interface IShapeTemplateHarvester
    {
        IEnumerable<string> SubPaths();
        IEnumerable<HarvestShapeHit> HarvestShape(HarvestShapeInfo info);
    }
}
