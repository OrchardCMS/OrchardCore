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
        public ShapeResult Shape<TModel>(Action<TModel> initialize) where TModel : class
        {
            return Shape<TModel>(shape => { initialize(shape); return Task.CompletedTask; });
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape<TModel>(Func<TModel, Task> initializeAsync) where TModel : class
        {
            return Shape<TModel>(
                typeof(TModel).Name,
                shape => initializeAsync(shape)
                );
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape<TModel>(string shapeType, Func<TModel, Task> initializeAsync) where TModel : class
        {
            return Shape(
                shapeType,
                ctx => ctx.ShapeFactory.CreateAsync(shapeType, initializeAsync)
                );
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape<TModel>(string shapeType, Action<TModel> initialize) where TModel : class
        {
            return Shape<TModel>(shapeType, shape => { initialize(shape); return Task.CompletedTask; });
        }

        /// <summary>
        /// Creates a new loosely typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape(string shapeType, Func<dynamic, Task> initializeAsync)
        {
            return Shape(shapeType, 
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
        public ShapeResult Shape(string shapeType, Action<dynamic> initialize)
        {
            return Shape(shapeType,
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
        public ShapeResult Shape(string shapeType)
        {
            return Shape(shapeType, ctx => ctx.ShapeFactory.CreateAsync(shapeType));
        }
        
        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name and initialized with a <see param name="model" />
        /// All the properties of the <see param name="model" /> object are duplicated on the resulting shape.
        /// </summary>
        public ShapeResult Shape<TModel>(string shapeType, TModel model) where TModel : class
        {
            return Shape(shapeType, ctx => ctx.ShapeFactory.CreateAsync(shapeType, model));
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created by the delegate.
        /// </summary>
        public ShapeResult Shape(string shapeType, Func<IBuildShapeContext, Task<IShape>> shapeBuilder)
        {
            return Shape(shapeType, shapeBuilder, null);
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created by the delegate.
        /// </summary>
        /// <remarks>
        /// This method is ultimately called by all drivers to create a shape. It's made virtual
        /// so that any concrete driver can use it as a way to alter any returning shape from the drivers.
        /// </remarks>
        public virtual ShapeResult Shape(string shapeType, Func<IBuildShapeContext, Task<IShape>> shapeBuilder, Func<IShape, Task> initializeAsync)
        {
            return new ShapeResult(shapeType, shapeBuilder, initializeAsync);
        }

        public CombinedResult Combine(params IDisplayResult[] results)
        {
            return new CombinedResult(results);
        }

        public CombinedResult Combine(IEnumerable<IDisplayResult> results)
        {
            return new CombinedResult( results );
        }
    }
}
