using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DisplayManagement.Handlers
{
    public class DisplayDriverBase
    {
        protected virtual string Prefix { get; set; } = "";

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape<TModel>(Func<TModel, Task> initializeAsync) where TModel : class
        {
            return new ShapeResult(
                typeof(TModel).Name,
                ctx => ctx.ShapeFactory.CreateAsync<TModel>(typeof(TModel).Name, 
                shape => initializeAsync(shape))
                ).Prefix(Prefix);
        }

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
        public ShapeResult Shape<TModel>(string shapeType, Func<TModel, Task> initializeAsync) where TModel : class
        {
            return new ShapeResult(
                shapeType,
                ctx => ctx.ShapeFactory.CreateAsync(shapeType, initializeAsync))
                .Prefix(Prefix);
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
            return new ShapeResult(shapeType, ctx => ctx.ShapeFactory.CreateAsync(shapeType, async () =>
            {
                var shape = new Shape();
                await initializeAsync(shape);
                return shape;
            })).Prefix(Prefix);
        }

        /// <summary>
        /// Creates a new loosely typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape(string shapeType, Action<dynamic> initialize)
        {
            return new ShapeResult(shapeType, ctx => ctx.ShapeFactory.CreateAsync(shapeType, () =>
            {
                var shape = new Shape();
                initialize(shape);
                return Task.FromResult<IShape>(shape);
            })).Prefix(Prefix);
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name.
        /// </summary>
        public ShapeResult Shape(string shapeType)
        {
            return new ShapeResult(shapeType, ctx => ctx.ShapeFactory.CreateAsync(shapeType)).Prefix(Prefix);
        }
        
        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name and initialized with a <see param name="model" />
        /// All the properties of the <see param name="model" /> object are duplicated on the resulting shape.
        /// </summary>
        public ShapeResult Shape<TModel>(string shapeType, TModel model) where TModel : class
        {
            return new ShapeResult(shapeType, ctx => ctx.ShapeFactory.CreateAsync(shapeType, model)).Prefix(Prefix);
        }


        /// <summary>
        /// If the shape needs to be rendered, it is created by the delegate.
        /// </summary>
        public ShapeResult Shape(string shapeType, Func<IBuildShapeContext, Task<IShape>> shapeBuilder)
        {
            return new ShapeResult(shapeType, shapeBuilder).Prefix(Prefix);
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created by the delegate.
        /// </summary>
        public ShapeResult Shape(string shapeType, Func<IBuildShapeContext, Task<IShape>> shapeBuilder, Func<dynamic, Task> initializeAsync)
        {
            return new ShapeResult(shapeType, shapeBuilder, initializeAsync).Prefix(Prefix);
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
