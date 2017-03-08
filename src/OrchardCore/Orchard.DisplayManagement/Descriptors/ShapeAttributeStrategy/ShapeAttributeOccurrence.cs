using System;
using System.Reflection;

namespace Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy
{
    public class ShapeAttributeOccurrence
    {
        public ShapeAttributeOccurrence(ShapeAttribute shapeAttribute, MethodInfo methodInfo, Type serviceType)
        {
            ShapeAttribute = shapeAttribute;
            MethodInfo = methodInfo;
            ServiceType = serviceType;
        }

        public ShapeAttribute ShapeAttribute { get; private set; }
        public MethodInfo MethodInfo { get; private set; }
        public Type ServiceType { get; private set; }
    }
}