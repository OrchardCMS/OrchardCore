using System.Collections.Generic;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Views
{
    public class ShapeViewModel : IShape, IPositioned
    {
        public ShapeViewModel()
        {
        }

        public ShapeViewModel(string shapeType)
        {
            Metadata.Type = shapeType;
        }

        private ShapeMetadata _metadata;
        public ShapeMetadata Metadata => _metadata = _metadata ?? new ShapeMetadata();

        public string Position
        {
            get
            {
                return Metadata.Position;
            }

            set
            {
                Metadata.Position = value;
            }
        }

        public string Id { get; set; }
        public string TagName { get; set; }

        private List<string> _classes;
        public IList<string> Classes => _classes = _classes ?? new List<string>();

        private Dictionary<string, string> _attributes;
        public IDictionary<string, string> Attributes => _attributes = _attributes ?? new Dictionary<string, string>();

        private Dictionary<string, object> _properties;
        public IDictionary<string, object> Properties => _properties = _properties ?? new Dictionary<string, object>();
    }
}
