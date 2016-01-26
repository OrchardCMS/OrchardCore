using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Shapes;
using Orchard.DisplayManagement.Views;
using System;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Handlers
{
    public class DisplayDriver<TModel> : IDisplayDriver
    {
        /// <summary>
        /// Creates a new strongly typed shape.
        /// </summary>
        public ShapeResult Shape<T>() where T : Shape, new()
        {
            return new ShapeResult(typeof(T).Name, ctx => ctx.ShapeFactory.Create<T>());
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape<T>(Func<T, Task> initialize) where T : Shape, new()
        {
            return new ShapeResult(
                typeof(T).Name, 
                ctx => ctx.ShapeFactory.Create<T>(),
                shape => initialize((T)shape)
                );
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name.
        /// </summary>
        public ShapeResult Shape(string shapeType)
        {
            return new ShapeResult(shapeType, ctx => ctx.ShapeFactory.Create(shapeType));
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
                );
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name and initialized with a model.
        /// </summary>
        public ShapeResult Shape(string shapeType, object model)
        {
            return new ShapeResult(shapeType, ctx => ctx.ShapeFactory.Create(shapeType, Arguments.From(model)));
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created by the delegate.
        /// </summary>
        public ShapeResult Shape(string shapeType, Func<IBuildShapeContext, dynamic> shapeBuilder)
        {
            return new ShapeResult(shapeType, shapeBuilder);
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created by the delegate.
        /// </summary>
        public ShapeResult Shape(string shapeType, Func<IBuildShapeContext, dynamic> shapeBuilder, Func<dynamic, Task> initialize)
        {
            return new ShapeResult(shapeType, shapeBuilder, initialize);
        }

        public CombinedResult Combine(params IDisplayResult[] results)
        {
            return new CombinedResult(results);
        }

        Task<IDisplayResult> IDisplayDriver.BuildDisplayAsync(object model, BuildDisplayContext context)
        {
            return DisplayAsync((TModel)model);
        }

        Task<IDisplayResult> IDisplayDriver.BuildEditorAsync(object model, BuildEditorContext context)
        {
            return EditAsync((TModel)model);

        }

        Task<IDisplayResult> IDisplayDriver.UpdateEditorAsync(object model, UpdateEditorContext context)
        {
            return UpdateAsync((TModel)model, context.Updater);
        }

        public virtual Task<IDisplayResult> DisplayAsync(TModel contentItem)
        {
            return Task.FromResult(Display(contentItem));
        }

        public virtual Task<IDisplayResult> EditAsync(TModel contentItem)
        {
            return Task.FromResult(Edit(contentItem));
        }

        public virtual Task<IDisplayResult> UpdateAsync(TModel contentItem, IUpdateModel updater)
        {
            return Task.FromResult(Update(contentItem, updater));
        }

        public virtual IDisplayResult Display(TModel contentItem)
        {
            return null;
        }

        public virtual IDisplayResult Edit(TModel contentItem)
        {
            return null;
        }

        public virtual IDisplayResult Update(TModel contentItem, IUpdateModel updater)
        {
            return null;
        }
    }
}
