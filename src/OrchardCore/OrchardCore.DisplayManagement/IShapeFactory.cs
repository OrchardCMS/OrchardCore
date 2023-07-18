using System;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DisplayManagement
{
    /// <summary>
    /// Service that creates new instances of dynamic shape objects
    /// This may be used directly, or through the IShapeHelperFactory.
    /// </summary>
    public interface IShapeFactory
    {
        ValueTask<IShape> CreateAsync(
            string shapeType,
            Func<ValueTask<IShape>> shapeFactory,
            Action<ShapeCreatingContext> creating,
            Action<ShapeCreatedContext> created);

        dynamic New { get; }
    }

    public static class ShapeFactoryExtensions
    {
        private static readonly ProxyGenerator _proxyGenerator = new();
        private static readonly Func<ValueTask<IShape>> _newShape = () => new(new Shape());

        /// <summary>
        /// Creates a new shape by copying the properties of the specific model.
        /// </summary>
        /// <param name="factory">The <see cref="IShapeFactory"/>.</param>
        /// <param name="shapeType">The type of shape to create.</param>
        /// <param name="model">The model to copy.</param>
        /// <returns></returns>
        public static ValueTask<IShape> CreateAsync<TModel>(this IShapeFactory factory, string shapeType, TModel model)
        {
            return factory.CreateAsync(shapeType, Arguments.From(model));
        }

        private static IShape CreateShape(Type baseType)
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
                options.AddMixinInstance(new ShapeViewModel());
                return (IShape)_proxyGenerator.CreateClassProxy(baseType, options);
            }
        }

        public static ValueTask<IShape> CreateAsync(this IShapeFactory factory, string shapeType)
        {
            return factory.CreateAsync(shapeType, _newShape);
        }

        public static ValueTask<IShape> CreateAsync(this IShapeFactory factory, string shapeType, Func<ValueTask<IShape>> shapeFactory)
        {
            return factory.CreateAsync(shapeType, shapeFactory, null, null);
        }

        /// <summary>
        /// Creates a dynamic proxy instance for the type and initializes it.
        /// </summary>
        /// <typeparam name="TModel">The type to instantiate.</typeparam>
        /// <param name="factory">The <see cref="IShapeFactory"/>.</param>
        /// <param name="shapeType">The shape type to create.</param>
        /// <param name="initializeAsync">The initialization method.</param>
        /// <returns></returns>
        public static ValueTask<IShape> CreateAsync<TModel>(this IShapeFactory factory, string shapeType, Func<TModel, ValueTask> initializeAsync)
        {
            static async ValueTask<IShape> Awaited(ValueTask task, IShape shape)
            {
                await task;
                return shape;
            }

            static ValueTask<IShape> ShapeFactory(Func<TModel, ValueTask> init)
            {
                var shape = CreateShape(typeof(TModel));
                var task = init((TModel)shape);
                if (!task.IsCompletedSuccessfully)
                {
                    return Awaited(task, shape);
                }

                return new ValueTask<IShape>(shape);
            }

            return factory.CreateAsync(shapeType, () => ShapeFactory(initializeAsync));
        }

        /// <summary>
        /// Creates a dynamic proxy instance for the type and initializes it.
        /// </summary>
        /// <typeparam name="TModel">The type to instantiate.</typeparam>
        /// <param name="factory">The <see cref="IShapeFactory"/>.</param>
        /// <param name="shapeType">The shape type to create.</param>
        /// <param name="initialize">The initialization method.</param>
        /// <returns></returns>
        public static ValueTask<IShape> CreateAsync<TModel>(this IShapeFactory factory, string shapeType, Action<TModel> initialize)
        {
            return factory.CreateAsync(shapeType, () =>
            {
                var shape = CreateShape(typeof(TModel));
                initialize((TModel)shape);
                return new ValueTask<IShape>(shape);
            });
        }

        public static ValueTask<IShape> CreateAsync<T>(this IShapeFactory factory, string shapeType, INamedEnumerable<T> parameters)
        {
            if (parameters == null || parameters == Arguments.Empty)
            {
                return factory.CreateAsync(shapeType);
            }

            return factory.CreateAsync(shapeType, _newShape, null, createdContext =>
            {
                var shape = (Shape)createdContext.Shape;

                // If only one non-Type, use it as the source object to copy

                var initializer = parameters.Positional.SingleOrDefault();

                if (initializer != null)
                {
                    // Use the Arguments class to optimize reflection code
                    var arguments = Arguments.From(initializer);

                    foreach (var prop in arguments.Named)
                    {
                        shape.Properties[prop.Key] = prop.Value;
                    }
                }
                else
                {
                    foreach (var kv in parameters.Named)
                    {
                        shape.Properties[kv.Key] = kv.Value;
                    }
                }
            });
        }
    }
}
