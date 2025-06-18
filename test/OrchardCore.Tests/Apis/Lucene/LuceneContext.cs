using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.Lucene;

public class LuceneContext : SiteContext
{
    public LuceneContext()
    {
        this.WithRecipe("luceneQueryTest");
    }
}
