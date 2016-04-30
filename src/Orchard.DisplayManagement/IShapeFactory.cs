using Orchard.DependencyInjection;
using Orchard.DisplayManagement.Shapes;
using System;

namespace Orchard.DisplayManagement
{
    /// <summary>
    /// Service that creates new instances of dynamic shape objects
    /// This may be used directly, or through the IShapeHelperFactory.
    /// </summary>
    public interface IShapeFactory : IDependency
    {
        T Create<T>(string shapeType) where T : class;
        object Create(Type type, string shapeType);
        T Create<T>(T obj) where T : class;
        IShape Create(string shapeType);
        IShape Create(string shapeType, INamedEnumerable<object> parameters);
        IShape Create(string shapeType, INamedEnumerable<object> parameters, Func<dynamic> createShape);
        dynamic New { get; }
    }
}