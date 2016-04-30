using System;
using System.Threading.Tasks;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.DisplayManagement.Handlers
{
    public class DisplayDriverBase
    {
        protected virtual string Prefix { get; set; } = "";

        /// <summary>
        /// Creates a new strongly typed shape.
        /// </summary>
        public ShapeResult Shape<TModel>() where TModel : class
        {
            return Shape<TModel>(typeof(TModel).Name);
        }

        /// <summary>
        /// Creates a new strongly typed shape.
        /// </summary>
        public ShapeResult Shape<TModel>(string shapeType) where TModel : class
        {
            return new ShapeResult(shapeType, ctx => ctx.ShapeFactory.Create<TModel>(shapeType)).Prefix(Prefix);
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape<TModel>(Func<TModel, Task> initialize) where TModel : class
        {
            return new ShapeResult(
                typeof(TModel).Name,
                ctx => ctx.ShapeFactory.Create<TModel>(typeof(TModel).Name),
                shape => initialize((TModel)shape)
                ).Prefix(Prefix);
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ShapeResult Shape<TModel>(string shapeType, Func<TModel, Task> initialize) where TModel : class
        {
            return new ShapeResult(
                shapeType,
                ctx => ctx.ShapeFactory.Create<TModel>(shapeType),
                shape => initialize((TModel)shape)
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
        /// If the shape needs to be rendered, it is created automatically from its type name and initialized with a model.
        /// </summary>
        public ShapeResult Shape(string shapeType, object model)
        {
            return new ShapeResult(shapeType, ctx => ctx.ShapeFactory.Create(shapeType, Arguments.From(model))).Prefix(Prefix);
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
    }

    public class DisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext> : DisplayDriverBase, IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>
        where TModel : class
        where TDisplayContext : BuildDisplayContext
        where TEditorContext : BuildEditorContext
        where TUpdateContext : UpdateEditorContext
    {

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.BuildDisplayAsync(TModel model, TDisplayContext context)
        {
            return DisplayAsync(model, context.Updater);
        }

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.BuildEditorAsync(TModel model, TEditorContext context)
        {
            return EditAsync(model, context.Updater);
        }

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.UpdateEditorAsync(TModel model, TUpdateContext context)
        {
            return UpdateAsync(model, context.Updater);
        }

        public virtual Task<IDisplayResult> DisplayAsync(TModel model, TDisplayContext context)
        {
            return DisplayAsync(model, context.Updater);
        }

        public virtual Task<IDisplayResult> EditAsync(TModel model, TEditorContext context)
        {
            return EditAsync(model, context.Updater);
        }

        public virtual Task<IDisplayResult> UpdateAsync(TModel model, TUpdateContext context)
        {
            return UpdateAsync(model, context.Updater);
        }

        public virtual Task<IDisplayResult> DisplayAsync(TModel model, IUpdateModel updater)
        {
            return Task.FromResult(Display(model, updater));
        }

        public virtual Task<IDisplayResult> EditAsync(TModel model, IUpdateModel updater)
        {
            return Task.FromResult(Edit(model, updater));
        }

        public virtual Task<IDisplayResult> UpdateAsync(TModel model, IUpdateModel updater)
        {
            return EditAsync(model, updater);
        }

        public virtual IDisplayResult Display(TModel model, TDisplayContext context)
        {
            return Display(model, context.Updater);
        }

        public virtual IDisplayResult Edit(TModel model, TEditorContext context)
        {
            return Edit(model, context.Updater);
        }

        public virtual IDisplayResult Display(TModel model, IUpdateModel updater)
        {
            return Display(model);
        }

        public virtual IDisplayResult Edit(TModel model, IUpdateModel updater)
        {
            return Edit(model);
        }

        public virtual IDisplayResult Display(TModel model)
        {
            return null;
        }

        public virtual IDisplayResult Edit(TModel model)
        {
            return null;
        }
    }
}
