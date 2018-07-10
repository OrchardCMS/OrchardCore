using System.Threading.Tasks;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.Services
{
    public abstract class FormHanderBase : IFormHandler
    {
        Task IFormHandler.SubmittedAsync(FormSubmittedContext context)
        {
            return OnSubmittedAsync(context);
        }

        protected virtual Task OnSubmittedAsync(FormSubmittedContext context)
        {
            OnSubmitted(context);
            return Task.CompletedTask;
        }

        protected virtual void OnSubmitted(FormSubmittedContext context)
        {
        }
    }
}
