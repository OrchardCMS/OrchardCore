using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement
{
    /// <summary>
    /// Service that creates new instances of dynamic shape objects
    /// This may be used directly, or through the IShapeHelperFactory.
    /// </summary>
    public interface IShapeFactory
    {
        IShape Create(string shapeType, Func<dynamic> shapeFactory);

        dynamic New { get; }
    }

    public static class ShapeFactoryExtensions
    {
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        public static IShape Create<TModel>(this IShapeFactory factory, string shapeType, TModel model)
        {
            return factory.Create(shapeType, Arguments.From(model));
        }

        private class ShapeImplementation : IShape, IPositioned
        {
            public ShapeMetadata Metadata { get; set; } = new ShapeMetadata();

            public string Position
            {
                get
                {
                    return Metadata.Position;
                }

                set
                {
                    Metadata.Position = value;
                }
            }

			public string Id { get; set; }
			public IList<string> Classes { get; } = new List<string>();
			public IDictionary<string, string> Attributes => new Dictionary<string,string>();
		}

        private static object CreateShape(Type baseType)
        {
            // Don't generate a proxy for shape types
            if (typeof(IShape).IsAssignableFrom(baseType))
            {
                var shape = Activator.CreateInstance(baseType) as IShape;
                return shape;
            }
            else
            {
                var options = new ProxyGenerationOptions();
                options.AddMixinInstance(new ShapeImplementation());
                return ProxyGenerator.CreateClassProxy(baseType, options);
            }
        }

        public static IShape Create(this IShapeFactory factory, string shapeType)
        {
            return factory.Create(shapeType, () => { return new Shape(); });
        }

        public static IShape Create<TShape>(this IShapeFactory factory, string shapeType, Action<TShape> initialize)
        {
            return factory.Create(shapeType, () =>
            {
                var shape = (TShape)CreateShape(typeof(TShape));
                initialize?.Invoke(shape);
                return shape;
            });
        }

        public static IShape Create(this IShapeFactory factory, Type baseType, string shapeType, Action<object> initialize)
        {
            return factory.Create(shapeType, () =>
            {
                var shape = CreateShape(baseType);
                initialize?.Invoke(shape);
                return shape;
            });
        }

        public static IShape Create(this IShapeFactory factory, string shapeType, INamedEnumerable<object> parameters)
        {
            return factory.Create(shapeType, () =>
            {
                var shape = new Shape();

                // If only one non-Type, use it as the source object to copy
                if (parameters != null)
                {
                    var initializer = parameters.Positional.SingleOrDefault();
                    if (initializer != null)
                    {
                        foreach (var prop in initializer.GetType().GetProperties())
                        {
                            shape.Properties[prop.Name] = prop.GetValue(initializer, null);
                        }
                    }
                    else
                    {
                        foreach (var kv in parameters.Named)
                        {
                            shape.Properties[kv.Key] = kv.Value;
                        }
                    }
                }

                return shape;
            });
        }
    }
}