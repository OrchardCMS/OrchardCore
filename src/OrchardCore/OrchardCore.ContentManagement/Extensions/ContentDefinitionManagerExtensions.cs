using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Infrastructure.Html;

namespace OrchardCore.ContentManagement.Extensions;

public static class ContentDefinitionManagerExtensions
{
    public static async Task SanitizeHtmlHolderAsync(
        this IContentDefinitionManager manager,
        IHtmlSanitizerService htmlSanitizerService,
        IHtmlHolderContent part,
        Func<ContentTypeDefinition, IHtmlHolderContent, bool> shouldSanitize = null)
    {
        var typeDefinition = await manager.GetTypeDefinitionAsync(part.ContentItem.ContentType);

        if (shouldSanitize == null || shouldSanitize(typeDefinition, part))
        {
            part.Html = htmlSanitizerService.Sanitize(part.Html);
        }
    }
}
