using Orchard.DependencyInjection;
using System;

namespace Orchard.DisplayManagement
{
    /// <summary>
    /// Service that creates new instances of dynamic shape objects
    /// This may be used directly, or through the IShapeHelperFactory.
    /// </summary>
    public interface IShapeFactory : IDependency
    {
        IShape Create(string shapeType);
        IShape Create(string shapeType, INamedEnumerable<object> parameters);
        IShape Create(string shapeType, INamedEnumerable<object> parameters, Func<dynamic> createShape);
    }
}