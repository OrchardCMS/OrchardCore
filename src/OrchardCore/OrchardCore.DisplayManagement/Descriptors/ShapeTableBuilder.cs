using System.Collections.Generic;
using System.Linq;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.DisplayManagement.Descriptors
{
    public class ShapeTableBuilder
    {
        private readonly IList<ShapeAlterationBuilder> _alterationBuilders = new List<ShapeAlterationBuilder>();
        private readonly IFeatureInfo _feature;

        public ShapeTableBuilder(IFeatureInfo feature, IReadOnlyCollection<string> excludedFeatureIds = null)
        {
            _feature = feature;
            ExcludedFeatureIds = excludedFeatureIds ?? new HashSet<string>();
        }

        public IReadOnlyCollection<string> ExcludedFeatureIds { get; }

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
