using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Indexing;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Contents.Indexing;

public class AspectsContentIndexHandler : IDocumentIndexHandler
{
    private readonly IContentManager _contentManager;

    public AspectsContentIndexHandler(IContentManager contentManager)
    {
        _contentManager = contentManager;
    }

    public async Task BuildIndexAsync(BuildDocumentIndexContext context)
    {
        if (context.Record is not ContentItem contentItem)
        {
            return;
        }

        var body = await _contentManager.PopulateAspectAsync(contentItem, new BodyAspect()).ConfigureAwait(false);

        if (body != null && body.Body != null)
        {
            context.DocumentIndex.Set(
                ContentIndexingConstants.BodyAspectBodyKey,
                body.Body,
                DocumentIndexOptions.Sanitize);
        }

        context.DocumentIndex.Set(
            ContentIndexingConstants.DisplayTextAnalyzedKey,
            contentItem.DisplayText,
            DocumentIndexOptions.Sanitize);

        // We need to store because of ContentPickerResultProvider(s)
        context.DocumentIndex.Set(
            ContentIndexingConstants.DisplayTextKey + ContentIndexingConstants.KeywordKey,
            contentItem.DisplayText,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        // We need to store because of ContentPickerResultProvider(s)
        context.DocumentIndex.Set(
            ContentIndexingConstants.DisplayTextNormalizedKey,
            contentItem.DisplayText?.ReplaceDiacritics().ToLower(),
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);
    }
}
