using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentLocalization.Handlers
{
    public interface IContentLocalizationPartHandler
    {
        Task LocalizingAsync(LocalizationContentContext context, ContentPart part);
        Task LocalizedAsync(LocalizationContentContext context, ContentPart part);
    }
}
