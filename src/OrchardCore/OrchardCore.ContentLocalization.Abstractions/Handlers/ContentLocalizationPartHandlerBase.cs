using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentLocalization.Handlers
{
    public abstract class ContentLocalizationPartHandlerBase<TPart> : IContentLocalizationPartHandler where TPart : ContentPart, new()
    {
        async Task IContentLocalizationPartHandler.LocalizingAsync(LocalizationContentContext context, ContentPart part)
        {
            if (part is TPart tpart)
            {
                await LocalizingAsync(context, tpart);
            }
        }

        async Task IContentLocalizationPartHandler.LocalizedAsync(LocalizationContentContext context, ContentPart part)
        {
            if (part is TPart tpart)
            {
                await LocalizedAsync(context, tpart);
            }
        }
        public virtual Task LocalizingAsync(LocalizationContentContext context, TPart part) => Task.CompletedTask;
        public virtual Task LocalizedAsync(LocalizationContentContext context, TPart part) => Task.CompletedTask;
    }
}
