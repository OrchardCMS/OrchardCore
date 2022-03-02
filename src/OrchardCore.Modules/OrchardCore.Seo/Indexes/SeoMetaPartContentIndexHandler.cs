using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Indexing;
using OrchardCore.Seo.Models;

namespace OrchardCore.Seo.Indexes;

public class SeoMetaPartContentIndexHandler : IContentItemIndexHandler
{
    public const string PageTitleKey = "Content.ContentItem.SeoMetaPart.PageTitle";

    public Task BuildIndexAsync(BuildIndexContext context)
    {
        var parent = context.ContentItem.As<SeoMetaPart>();

        if (parent == null)
        {
            return Task.CompletedTask;
        }

        context.DocumentIndex.Set(
            PageTitleKey,
            parent.PageTitle,
            DocumentIndexOptions.Store);

        return Task.CompletedTask;
    }
}
