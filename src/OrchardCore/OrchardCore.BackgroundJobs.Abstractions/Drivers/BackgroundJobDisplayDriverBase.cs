using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.BackgroundJobs.Drivers
{
    public abstract class BackgroundJobDisplayDriverBase<TConcrete> :
      DisplayDriver<TConcrete, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>,
      IDisplayDriver<BackgroundJobExecution>,
      IDisplayDriver<BackgroundJobExecution, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>
      where TConcrete : class, IBackgroundJob
    {
        public virtual bool CanHandleModel(BackgroundJobExecution model)
            => true;

        Task<IDisplayResult> IDisplayDriver<BackgroundJobExecution, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>.BuildDisplayAsync(BackgroundJobExecution model, BuildDisplayContext context)
        {
            var concrete = model.BackgroundJob as TConcrete;

            if (concrete == null || !CanHandleModel(concrete))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            BuildPrefix(concrete, context.HtmlFieldPrefix);

            return DisplayAsync(concrete, context);
        }

        Task<IDisplayResult> IDisplayDriver<BackgroundJobExecution, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>.BuildEditorAsync(BackgroundJobExecution model, BuildEditorContext context)
        {
            var concrete = model.BackgroundJob as TConcrete;

            if (concrete == null || !CanHandleModel(concrete))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            BuildPrefix(concrete, context.HtmlFieldPrefix);

            return EditAsync(concrete, context);
        }

        Task<IDisplayResult> IDisplayDriver<BackgroundJobExecution, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>.UpdateEditorAsync(BackgroundJobExecution model, UpdateEditorContext context)
        {
            var concrete = model.BackgroundJob as TConcrete;

            if (concrete == null || !CanHandleModel(concrete))
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            BuildPrefix(concrete, context.HtmlFieldPrefix);

            return UpdateAsync(concrete, context);
        }
    }
}
