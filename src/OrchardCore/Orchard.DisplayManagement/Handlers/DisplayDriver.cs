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
    }

    public abstract class DisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext> : DisplayDriverBase, IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>
        where TModel : class
        where TDisplayContext : BuildDisplayContext
        where TEditorContext : BuildEditorContext
        where TUpdateContext : UpdateEditorContext
    {

        /// <summary>
        /// Returns a unique prefix based on the model.
        /// </summary>
        public abstract string GeneratePrefix(TModel model);

        /// <summary>
        /// Returns <c>true</c> if the model can be handle by the current driver.
        /// </summary>
        /// <returns></returns>
        public abstract bool CanHandleModel(TModel model);

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.BuildDisplayAsync(TModel model, TDisplayContext context)
        {
            if(!CanHandleModel(model))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            Prefix = GeneratePrefix(model);

            if (!String.IsNullOrEmpty(context.HtmlFieldPrefix))
            {
                Prefix = context.HtmlFieldPrefix + "." + Prefix;
            }

            return DisplayAsync(model, context);
        }

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.BuildEditorAsync(TModel model, TEditorContext context)
        {
            if (!CanHandleModel(model))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            Prefix = GeneratePrefix(model);

            if (!String.IsNullOrEmpty(context.HtmlFieldPrefix))
            {
                Prefix = context.HtmlFieldPrefix + "." + Prefix;
            }

            return EditAsync(model, context);
        }

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.UpdateEditorAsync(TModel model, TUpdateContext context)
        {
            if (!CanHandleModel(model))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            Prefix = GeneratePrefix(model);

            if (!String.IsNullOrEmpty(context.HtmlFieldPrefix))
            {
                Prefix = context.HtmlFieldPrefix + "." + Prefix;
            }

            return UpdateAsync(model, context);
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
