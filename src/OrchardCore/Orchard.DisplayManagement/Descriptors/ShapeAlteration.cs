using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Features;

namespace Orchard.DisplayManagement.Descriptors
{
    public class ShapeAlteration
    {
        private readonly IList<Action<ShapeDescriptor>> _configurations;

        public ShapeAlteration(string shell, string shapeType, IFeatureInfo feature, IList<Action<ShapeDescriptor>> configurations)
        {
            _configurations = configurations;
            Shell = shell;
            ShapeType = shapeType;
            Feature = feature;
        }

        public string Shell { get; private set; } = string.Empty;
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