using Orchard.ContentManagement;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.Demo.Models
{
    public class TestContentPartA : ContentPart
    {
        public ShapeMetadata Metadata { get; set; }
        public string Line { get; set; }
    }
}