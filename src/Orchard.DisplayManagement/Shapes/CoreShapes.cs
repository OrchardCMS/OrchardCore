using Orchard.DisplayManagement.Descriptors;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Shapes
{
    public class CoreShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
        }

        [Shape]
        public void PlaceChildContent(dynamic Source, TextWriter Output)
        {
            throw new NotImplementedException();
        }
    }
}
