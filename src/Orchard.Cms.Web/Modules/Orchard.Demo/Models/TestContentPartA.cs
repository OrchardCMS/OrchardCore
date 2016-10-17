using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.Demo.Models
{
    public class TestContentPartA : ContentPart, IShape
    {
        public ShapeMetadata Metadata { get; set; }
        public string Line { get; set; }
    }
}