using Microsoft.AspNet.Html.Abstractions;
using Orchard.DisplayManagement.Descriptors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
