using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DisplayManagement.Handlers
{
    public class DisplayDriverBase
    {
        protected string Prefix { get; set; } = "";

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Initialize<TModel>(Action<TModel> initialize) where TModel : class
        {
            return Initialize<TModel>(shape =>
            {
                initialize(shape);
                return new ValueTask();
            });
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Initialize<TModel>(Func<TModel, ValueTask> initializeAsync) where TModel : class
        {
            return Initialize<TModel>(
                typeof(TModel).Name,
                shape => initializeAsync(shape)
                );
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Initialize<TModel>(string shapeType, Func<TModel, ValueTask> initializeAsync) where TModel : class
        {
            return Factory(
                shapeType,
                ctx => ctx.ShapeFactory.CreateAsync(shapeType, initializeAsync)
                );
        }

        /// <summary>
        /// Creates a dynamic proxy for the specified model. Properties are copied to the new object.
        /// </summary>
        public ShapeResult Copy<TModel>(string shapeType, TModel model) where TModel : class
        {
            return Factory(shapeType, ctx => ctx.ShapeFactory.CreateAsync(shapeType, model));
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Initialize<TModel>(string shapeType, Action<TModel> initialize) where TModel : class
        {
            return Initialize<TModel>(shapeType, shape =>
            {
                initialize(shape);
                return new ValueTask();
            });
        }

        /// <summary>
        /// Creates a new loosely typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Dynamic(string shapeType, Func<dynamic, Task> initializeAsync)
        {
            return Factory(shapeType,
                async ctx =>
                {
                    dynamic shape = await ctx.ShapeFactory.CreateAsync(shapeType);
                    await initializeAsync(shape);
                    return shape;
                });
        }

        /// <summary>
        /// Creates a new loosely typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Dynamic(string shapeType, Action<dynamic> initialize)
        {
            return Factory(shapeType,
                async ctx =>
                {
                    dynamic shape = await ctx.ShapeFactory.CreateAsync(shapeType);
                    initialize(shape);
                    return shape;
                });
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name.
        /// </summary>
        public ShapeResult Dynamic(string shapeType)
        {
            return Factory(shapeType, ctx => ctx.ShapeFactory.CreateAsync(shapeType));
        }

        /// <summary>
        /// Creates a <see cref="ShapeViewModel{TModel}"/> for the specific model.
        /// </summary>
        public ShapeResult View<TModel>(string shapeType, TModel model) where TModel : class
        {
            return Factory(shapeType, ctx => new ValueTask<IShape>(new ShapeViewModel<TModel>(model)));
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name and initialized.
        /// </summary>
        public ShapeResult Shape(string shapeType, IShape shape)
        {
            return Factory(shapeType, ctx => new ValueTask<IShape>(shape));
        }

        /// <summary>
        /// Creates a shape lazily.
        /// </summary>
        public ShapeResult Factory(string shapeType, Func<IBuildShapeContext, ValueTask<IShape>> shapeBuilder)
        {
            return Factory(shapeType, shapeBuilder, null);
        }

        /// <summary>
        /// Creates a shape lazily.
        /// </summary>
        public ShapeResult Factory(string shapeType, Func<IBuildShapeContext, IShape> shapeBuilder)
        {
            return Factory(shapeType, ctx => new ValueTask<IShape>(shapeBuilder(ctx)), null);
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created by the delegate.
        /// </summary>
        /// <remarks>
        /// This method is ultimately called by all drivers to create a shape. It's made virtual
        /// so that any concrete driver can use it as a way to alter any returning shape from the drivers.
        /// </remarks>
        public virtual ShapeResult Factory(string shapeType, Func<IBuildShapeContext, ValueTask<IShape>> shapeBuilder, Func<IShape, Task> initializeAsync)
        {
            return new ShapeResult(shapeType, shapeBuilder, initializeAsync)
                .Prefix(Prefix);
        }

        public static CombinedResult Combine(params IDisplayResult[] results) => new(results);

        public static CombinedResult Combine(IEnumerable<IDisplayResult> results) => new(results);
    }
}
