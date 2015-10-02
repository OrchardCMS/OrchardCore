using System;
using System.Reflection;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy {
    public class ShapeAttributeOccurrence {
        private readonly Func<Feature> _feature;

        public ShapeAttributeOccurrence(ShapeAttribute shapeAttribute, MethodInfo methodInfo, IServiceProvider registration, Func<Feature> feature) {
            ShapeAttribute = shapeAttribute;
            MethodInfo = methodInfo;
            Registration = registration;
            _feature = feature;
        }

        public ShapeAttribute ShapeAttribute { get; private set; }
        public MethodInfo MethodInfo { get; private set; }
        public IServiceProvider Registration { get; private set; }
        public Feature Feature { get { return _feature(); } }
    }
}