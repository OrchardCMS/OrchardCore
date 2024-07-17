using System.Threading.Tasks;

namespace OrchardCore.ContentLocalization.Handlers
{
    public class ContentLocalizationHandlerBase : IContentLocalizationHandler
    {
        public virtual Task LocalizedAsync(LocalizationContentContext context) => Task.CompletedTask;
        public virtual Task LocalizingAsync(LocalizationContentContext context) => Task.CompletedTask;
    }
}
