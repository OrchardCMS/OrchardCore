using Orchard.Environment.Extensions.Features;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Descriptors
{
    public class ShapeTableBuilder
    {
        private readonly IList<ShapeAlterationBuilder> _alterationBuilders = new List<ShapeAlterationBuilder>();
        private readonly IFeatureInfo _feature;

        public ShapeTableBuilder(IFeatureInfo feature)
        {
            _feature = feature;
        }

        public ShapeAlterationBuilder Describe(string shapeType)
        {
            var alterationBuilder = new ShapeAlterationBuilder(_feature, shapeType);
            _alterationBuilders.Add(alterationBuilder);
            return alterationBuilder;
        }

        public IEnumerable<ShapeAlteration> BuildAlterations()
        {
            return _alterationBuilders.Select(b => b.Build());
        }
    }
}