using Orchard.DisplayManagement.Descriptors;
using System;
using System.IO;

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
