using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Indexing;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Contents.Indexing;

public class AspectsContentIndexHandler : IContentItemIndexHandler
{
    private readonly IContentManager _contentManager;

    public AspectsContentIndexHandler(IContentManager contentManager)
    {
        _contentManager = contentManager;
    }

    public async Task BuildIndexAsync(BuildIndexContext context)
    {
        var body = await _contentManager.PopulateAspectAsync(context.ContentItem, new BodyAspect());

        if (body != null && body.Body != null)
        {
            context.DocumentIndex.Set(
                ContentIndexingConstants.BodyAspectBodyKey,
                body.Body,
                DocumentIndexOptions.Sanitize);
        }

        context.DocumentIndex.Set(
            ContentIndexingConstants.DisplayTextAnalyzedKey,
            context.ContentItem.DisplayText,
            DocumentIndexOptions.Sanitize);

        // We need to store because of ContentPickerResultProvider(s)
        context.DocumentIndex.Set(
            ContentIndexingConstants.DisplayTextKey + ContentIndexingConstants.KeywordKey,
            context.ContentItem.DisplayText,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        // We need to store because of ContentPickerResultProvider(s)
        context.DocumentIndex.Set(
            ContentIndexingConstants.DisplayTextNormalizedKey,
            context.ContentItem.DisplayText?.ReplaceDiacritics().ToLower(),
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);
    }
}
