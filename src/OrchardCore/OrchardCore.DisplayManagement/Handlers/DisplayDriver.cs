using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DisplayManagement.Handlers
{
    public abstract class DisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext> :
        DisplayDriverBase,
        IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>
        where TModel : class
        where TDisplayContext : BuildDisplayContext
        where TEditorContext : BuildEditorContext
        where TUpdateContext : UpdateEditorContext
    {
        protected static readonly string ModelName = typeof(TModel).Name;

        /// <summary>
        /// Returns <see langword="true"/> if the model can be handled by the current driver.
        /// </summary>
        public virtual bool CanHandleModel(TModel model)
        {
            return true;
        }

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.BuildDisplayAsync(TModel model, TDisplayContext context)
        {
            if (!CanHandleModel(model))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            BuildPrefix(model, context.HtmlFieldPrefix);

            return DisplayAsync(model, context);
        }

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.BuildEditorAsync(TModel model, TEditorContext context)
        {
            if (!CanHandleModel(model))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            BuildPrefix(model, context.HtmlFieldPrefix);

            return EditAsync(model, context);
        }

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.UpdateEditorAsync(TModel model, TUpdateContext context)
        {
            if (!CanHandleModel(model))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            BuildPrefix(model, context.HtmlFieldPrefix);

            return UpdateAsync(model, context);
        }

        public virtual Task<IDisplayResult> DisplayAsync(TModel model, TDisplayContext context)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return DisplayAsync(model, context.Updater);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Obsolete("This method is obsolete and will be removed in version 3. Instead, use the DisplayAsync(TModel model, TDisplayContext context) method.")]
        public virtual Task<IDisplayResult> DisplayAsync(TModel model, IUpdateModel updater)
        {
            return Task.FromResult(Display(model, updater));
        }

        [Obsolete("This method is obsolete and will be removed in version 3. Instead, use the DisplayAsync(TModel model, TDisplayContext context) method.")]
        public virtual IDisplayResult Display(TModel model, IUpdateModel updater)
        {
            return Display(model);
        }

        [Obsolete("This method is obsolete and will be removed in version 3. Instead, use the DisplayAsync(TModel model, TDisplayContext context) method.")]
        public virtual IDisplayResult Display(TModel model)
        {
            return NullShapeResult();
        }

        public virtual Task<IDisplayResult> EditAsync(TModel model, TEditorContext context)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return EditAsync(model, context.Updater);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Obsolete("This method is obsolete and will be removed in version 3. Instead, use the EditAsync(TModel model, TEditorContext context) method.")]
        public virtual Task<IDisplayResult> EditAsync(TModel model, IUpdateModel updater)
        {
            return Task.FromResult(Edit(model, updater));
        }

        [Obsolete("This method is obsolete and will be removed in version 3. Instead, use the EditAsync(TModel model, TEditorContext context) method.")]
        public virtual IDisplayResult Edit(TModel model, IUpdateModel updater)
        {
            return Edit(model);
        }

        [Obsolete("This method is obsolete and will be removed in version 3. Instead, use the EditAsync(TModel model, TEditorContext context) method.")]
        public virtual IDisplayResult Edit(TModel model)
        {
            return NullShapeResult();
        }

        private static IDisplayResult NullShapeResult()
        {
            return null;
        }

        public virtual Task<IDisplayResult> UpdateAsync(TModel model, TUpdateContext context)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return UpdateAsync(model, context.Updater);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Obsolete("This method is obsolete and will be removed in version 3. Instead, use the UpdateAsync(TModel model, TUpdateContext context) method.")]
        public virtual Task<IDisplayResult> UpdateAsync(TModel model, IUpdateModel updater)
        {
            return EditAsync(model, updater);
        }

        protected virtual void BuildPrefix(TModel model, string htmlFieldPrefix)
        {
            if (!string.IsNullOrEmpty(htmlFieldPrefix))
            {
                Prefix = $"{htmlFieldPrefix}.{ModelName}";
            }
            else
            {
                Prefix = ModelName;
            }
        }
    }

    public abstract class DisplayDriver<TModel> :
        DisplayDriver<TModel, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>,
        IDisplayDriver<TModel>
        where TModel : class
    {
    }

    public abstract class DisplayDriver<TModel, TConcrete, TDisplayContext, TEditorContext, TUpdateContext> :
        DisplayDriver<TConcrete, TDisplayContext, TEditorContext, TUpdateContext>,
        IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>
        where TConcrete : class, TModel
        where TModel : class
        where TDisplayContext : BuildDisplayContext
        where TEditorContext : BuildEditorContext
        where TUpdateContext : UpdateEditorContext
    {
        /// <summary>
        /// Returns <c>true</c> if the model can be handle by the current driver.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanHandleModel(TModel model)
        {
            return true;
        }

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.BuildDisplayAsync(TModel model, TDisplayContext context)
        {
            var concrete = model as TConcrete;

            if (concrete == null || !CanHandleModel(concrete))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            BuildPrefix(concrete, context.HtmlFieldPrefix);

            return DisplayAsync(concrete, context);
        }

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.BuildEditorAsync(TModel model, TEditorContext context)
        {
            var concrete = model as TConcrete;

            if (concrete == null || !CanHandleModel(concrete))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            BuildPrefix(concrete, context.HtmlFieldPrefix);

            return EditAsync(concrete, context);
        }

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.UpdateEditorAsync(TModel model, TUpdateContext context)
        {
            var concrete = model as TConcrete;

            if (concrete == null || !CanHandleModel(concrete))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            BuildPrefix(concrete, context.HtmlFieldPrefix);

            return UpdateAsync(concrete, context);
        }
    }

    public abstract class DisplayDriver<TModel, TConcrete> :
        DisplayDriver<TModel, TConcrete, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>,
        IDisplayDriver<TModel>
        where TConcrete : class, TModel
        where TModel : class
    {
    }
}
