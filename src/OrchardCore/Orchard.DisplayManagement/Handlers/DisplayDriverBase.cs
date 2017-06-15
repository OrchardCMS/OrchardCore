using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orchard.DisplayManagement.Views;

namespace Orchard.DisplayManagement.Handlers
{
    public class DisplayDriverBase
    {
        protected virtual string Prefix { get; set; } = "";

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape<TModel>(Func<TModel, Task> initialize) where TModel : class
        {
            return new ShapeResult(
                typeof(TModel).Name,
                ctx => ctx.ShapeFactory.Create<TModel>(typeof(TModel).Name, 
                shape => initialize(shape))
                ).Prefix(Prefix);
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape<TModel>(Action<TModel> initialize) where TModel : class
        {
            return Shape<TModel>(shape => { initialize((TModel)shape); return Task.CompletedTask; });
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape<TModel>(string shapeType, Func<TModel, Task> initialize) where TModel : class
        {
            return new ShapeResult(
                shapeType,
                ctx => ctx.ShapeFactory.Create<TModel>(shapeType, shape => initialize(shape)))
                .Prefix(Prefix);
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape<TModel>(string shapeType, Action<TModel> initialize) where TModel : class
        {
            return Shape<TModel>(shapeType, shape => { initialize((TModel)shape); return Task.CompletedTask; });
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape(Type baseType, string shapeType, Action<object> initialize)
        {
            return new ShapeResult(
                shapeType,
                ctx => ctx.ShapeFactory.Create(baseType, shapeType, shape => initialize(shape))
                ).Prefix(Prefix);
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name.
        /// </summary>
        public ShapeResult Shape(string shapeType)
        {
            return new ShapeResult(shapeType, ctx => ctx.ShapeFactory.Create(shapeType)).Prefix(Prefix);
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name.
        /// </summary>
        public ShapeResult Shape(string shapeType, Func<dynamic, Task> initialize)
        {
            return new ShapeResult(
                shapeType,
                ctx => ctx.ShapeFactory.Create(shapeType),
                initialize
                ).Prefix(Prefix);
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name and initialized with a <see param name="model" />
        /// All the properties of the <see param name="model" /> object are duplicated on the resulting shape.
        /// </summary>
        public ShapeResult Shape<TModel>(string shapeType, TModel model) where TModel : class
        {
            return new ShapeResult(shapeType, ctx => ctx.ShapeFactory.Create(shapeType, model)).Prefix(Prefix);
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created by the delegate.
        /// </summary>
        public ShapeResult Shape(string shapeType, Func<IBuildShapeContext, dynamic> shapeBuilder)
        {
            return new ShapeResult(shapeType, shapeBuilder).Prefix(Prefix);
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created by the delegate.
        /// </summary>
        public ShapeResult Shape(string shapeType, Func<IBuildShapeContext, dynamic> shapeBuilder, Func<dynamic, Task> initialize)
        {
            return new ShapeResult(shapeType, shapeBuilder, initialize).Prefix(Prefix);
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
