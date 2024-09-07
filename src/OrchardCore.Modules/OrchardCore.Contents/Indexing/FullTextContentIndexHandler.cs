using Cysharp.Text;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Indexing;

namespace OrchardCore.Contents.Indexing;

public class FullTextContentIndexHandler(IContentManager contentManager) : IContentItemIndexHandler
{
    private readonly IContentManager _contentManager = contentManager;

    public async Task BuildIndexAsync(BuildIndexContext context)
    {
        var result = await _contentManager.PopulateAspectAsync<FullTextAspect>(context.ContentItem);

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

        context.DocumentIndex.Set(IndexingConstants.FullTextKey, value, DocumentIndexOptions.Sanitize);
    }
}
