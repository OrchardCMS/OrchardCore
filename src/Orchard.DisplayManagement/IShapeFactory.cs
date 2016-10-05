using Orchard.DependencyInjection;
using System;

namespace Orchard.DisplayManagement
{
    /// <summary>
    /// Service that creates new instances of dynamic shape objects
    /// This may be used directly, or through the IShapeHelperFactory.
    /// </summary>
    public interface IShapeFactory
    {
        T Create<T>(string shapeType) where T : class;
        object Create(Type type, string shapeType);
        T Create<T>(T obj) where T : class;
        IShape Create(string shapeType);
        IShape Create(string shapeType, INamedEnumerable<object> parameters);
        IShape Create(string shapeType, INamedEnumerable<object> parameters, Func<dynamic> createShape);
        dynamic New { get; }
    }

    public static class ShapeFactoryExtensions
    {
        public static IShape Create(this IShapeFactory factory, string shapeType, object parameters)
        {
            return factory.Create(shapeType, Arguments.From(parameters));
        }
    }

}