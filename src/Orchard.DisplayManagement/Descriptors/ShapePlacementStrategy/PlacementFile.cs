using System.Collections.Generic;

namespace Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy {
    public class PlacementFile : PlacementNode {
    }

    public class PlacementNode {
        public IEnumerable<PlacementNode> Nodes { get; set; }
    }

    public class PlacementMatch : PlacementNode {
        public IDictionary<string, string> Terms { get; set; }
    }

    public class PlacementShapeLocation : PlacementNode {
        public string ShapeType { get; set; }
        public string Location { get; set; }
    }
}