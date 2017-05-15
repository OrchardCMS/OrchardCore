using System;
using System.Threading.Tasks;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.DisplayManagement.Handlers
{
    public abstract class DisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext> : 
        DisplayDriverBase, 
        IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>
        where TModel : class
        where TDisplayContext : BuildDisplayContext
        where TEditorContext : BuildEditorContext
        where TUpdateContext : UpdateEditorContext
    {

        /// <summary>
        /// Returns a unique prefix based on the model.
        /// </summary>
        public virtual string GeneratePrefix(TModel model)
        {
            return typeof(TModel).Name;
        }

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

        public virtual Task<IDisplayResult> DisplayAsync(TModel model, IUpdateModel updater)
        {
            return Task.FromResult(Display(model, updater));
        }

        public virtual IDisplayResult Display(TModel model, IUpdateModel updater)
        {
            return Display(model);
        }

        public virtual IDisplayResult Display(TModel model)
        {
            return null;
        }

        public virtual Task<IDisplayResult> EditAsync(TModel model, TEditorContext context)
        {
            return EditAsync(model, context.Updater);
        }

        public virtual Task<IDisplayResult> EditAsync(TModel model, IUpdateModel updater)
        {
            return Task.FromResult(Edit(model, updater));
        }
        
        public virtual IDisplayResult Edit(TModel model, IUpdateModel updater)
        {
            return Edit(model);
        }

        public virtual IDisplayResult Edit(TModel model)
        {
            return null;
        }

        public virtual Task<IDisplayResult> UpdateAsync(TModel model, TUpdateContext context)
        {
            return UpdateAsync(model, context.Updater);
        }
        
        public virtual Task<IDisplayResult> UpdateAsync(TModel model, IUpdateModel updater)
        {
            return EditAsync(model, updater);
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
        where TConcrete: class, TModel
        where TModel: class
        where TDisplayContext : BuildDisplayContext
        where TEditorContext : BuildEditorContext
        where TUpdateContext : UpdateEditorContext
    {
        /// <summary>
        /// Returns a unique prefix based on the model.
        /// </summary>
        public virtual string GeneratePrefix(TModel model)
        {
            return typeof(TModel).Name;
        }

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

            Prefix = GeneratePrefix(concrete);

            if (!String.IsNullOrEmpty(context.HtmlFieldPrefix))
            {
                Prefix = context.HtmlFieldPrefix + "." + Prefix;
            }

            return DisplayAsync(concrete, context);
        }

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.BuildEditorAsync(TModel model, TEditorContext context)
        {
            var concrete = model as TConcrete;

            if (concrete == null || !CanHandleModel(concrete))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            Prefix = GeneratePrefix(concrete);

            if (!String.IsNullOrEmpty(context.HtmlFieldPrefix))
            {
                Prefix = context.HtmlFieldPrefix + "." + Prefix;
            }

            return EditAsync(concrete, context);
        }

        Task<IDisplayResult> IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>.UpdateEditorAsync(TModel model, TUpdateContext context)
        {
            var concrete = model as TConcrete;

            if (concrete == null || !CanHandleModel(concrete))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            Prefix = GeneratePrefix(concrete);

            if (!String.IsNullOrEmpty(context.HtmlFieldPrefix))
            {
                Prefix = context.HtmlFieldPrefix + "." + Prefix;
            }

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
