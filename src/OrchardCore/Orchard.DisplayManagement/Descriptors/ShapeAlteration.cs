using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Features;

namespace Orchard.DisplayManagement.Descriptors
{
    public class ShapeAlteration
    {
        private readonly IList<Action<ShapeDescriptor>> _configurations;

        public ShapeAlteration(string shapeType, IFeatureInfo feature, IList<Action<ShapeDescriptor>> configurations)
        {
            _configurations = configurations;
            ShapeType = shapeType;
            Feature = feature;
        }

        public string ShapeType { get; private set; }
        public IFeatureInfo Feature { get; private set; }
        public void Alter(ShapeDescriptor descriptor)
        {
            foreach (var configuration in _configurations)
            {
                configuration(descriptor);
            }
        }
    }
}