using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.Lucene
{
    public class LuceneContext : SiteContext
    {
        static LuceneContext()
        {
        }

        public LuceneContext()
        {
            this.WithRecipe("luceneQueryTest");
        }
    }
}
