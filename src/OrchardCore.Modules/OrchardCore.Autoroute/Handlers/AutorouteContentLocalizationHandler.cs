using System.Threading.Tasks;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentLocalization.Handlers;

internal sealed class AutorouteContentLocalizationHandler : IContentLocalizationHandler
{
    public Task LocalizedAsync(LocalizationContentContext context) => Task.CompletedTask;

    public Task LocalizingAsync(LocalizationContentContext context)
    {
        if (context.ContentItem.Has<AutoroutePart>())
        {
            // Clearing the AutoroutePart path to regenerate the permalink automatically.
            context.ContentItem.Alter<AutoroutePart>(p => p.Path = null);
        }

        return Task.CompletedTask;
    }
}
