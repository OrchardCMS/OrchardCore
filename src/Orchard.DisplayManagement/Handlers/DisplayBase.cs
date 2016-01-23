using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Shapes;
using Orchard.DisplayManagement.Views;
using System;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Handlers
{
    public class DisplayBase<TModel> : IDisplay<TModel>
    {
        /// <summary>
        /// Creates a new strongly typed shape.
        /// </summary>
        public ShapeResult<TModel> Shape<T>() where T : Shape, new()
        {
            return new ShapeResult<TModel>(typeof(T).Name, ctx => ctx.ShapeFactory.Create<T>());
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult<TModel> Shape<T>(Func<T, Task> initialize) where T : Shape, new()
        {
            return new ShapeResult<TModel>(
                typeof(T).Name, 
                ctx => ctx.ShapeFactory.Create<T>(),
                shape => initialize((T)shape)
                );
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name.
        /// </summary>
        public ShapeResult<TModel> Shape(string shapeType)
        {
            return new ShapeResult<TModel>(shapeType, ctx => ctx.ShapeFactory.Create(shapeType));
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name.
        /// </summary>
        public ShapeResult<TModel> Shape(string shapeType, Func<dynamic, Task> initialize)
        {
            return new ShapeResult<TModel>(
                shapeType, 
                ctx => ctx.ShapeFactory.Create(shapeType),
                initialize
                );
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name and initialized with a model.
        /// </summary>
        public ShapeResult<TModel> Shape(string shapeType, object model)
        {
            return new ShapeResult<TModel>(shapeType, ctx => ctx.ShapeFactory.Create(shapeType, Arguments.From(model)));
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created by the delegate.
        /// </summary>
        public ShapeResult<TModel> Shape(string shapeType, Func<BuildShapeContext<TModel>, dynamic> shapeBuilder)
        {
            return new ShapeResult<TModel>(shapeType, shapeBuilder);
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created by the delegate.
        /// </summary>
        public ShapeResult<TModel> Shape(string shapeType, Func<BuildShapeContext<TModel>, dynamic> shapeBuilder, Func<dynamic, Task> initialize)
        {
            return new ShapeResult<TModel>(shapeType, shapeBuilder, initialize);
        }

        public CombinedResult<TModel> Combine(params DisplayResult<TModel>[] results)
        {
            return new CombinedResult<TModel>(results);
        }

        public virtual Task<DisplayResult<TModel>> BuildDisplayAsync(BuildDisplayContext<TModel> context)
        {
            return Task.FromResult(BuildDisplay(context));
        }

        public virtual Task<DisplayResult<TModel>> BuildEditorAsync(BuildEditorContext<TModel> context)
        {
            return Task.FromResult(BuildEditor(context));
        }

        public virtual Task<DisplayResult<TModel>> UpdateEditorAsync(UpdateEditorContext<TModel> context, IUpdateModel updater)
        {
            return Task.FromResult(UpdateEditor(context, updater));
        }

        public virtual DisplayResult<TModel> BuildDisplay(BuildDisplayContext<TModel> context)
        {
            return null;
        }

        public virtual DisplayResult<TModel> BuildEditor(BuildEditorContext<TModel> context)
        {
            return null;
        }

        public virtual DisplayResult<TModel> UpdateEditor(UpdateEditorContext<TModel> context, IUpdateModel updater)
        {
            return null;
        }
    }
}
