using Cysharp.Text;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Indexing;

namespace OrchardCore.Contents.Indexing;

public class FullTextContentIndexHandler(IContentManager contentManager) : IDocumentIndexHandler
{
    private readonly IContentManager _contentManager = contentManager;

    public async Task BuildIndexAsync(BuildDocumentIndexContext context)
    {
        if (context.Record is not ContentItem contentItem)
        {
            return;
        }

        var result = await _contentManager.PopulateAspectAsync<FullTextAspect>(contentItem);

        using var stringBuilder = ZString.CreateStringBuilder();

        foreach (var segment in result.Segments)
        {
            stringBuilder.Append(segment);
            stringBuilder.Append(" ");
        }

        var value = stringBuilder.ToString();

        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        context.DocumentIndex.Set(ContentIndexingConstants.FullTextKey, value, DocumentIndexOptions.Sanitize);
    }
}
