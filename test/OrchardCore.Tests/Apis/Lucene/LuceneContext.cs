using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Lucene;
using OrchardCore.Testing.Context;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.Lucene
{
    public class LuceneContext : SiteContext
    {
        static LuceneContext()
        {
            SiteContextConfig.Recipies = new RecipeLocator[]
            {
                new RecipeLocator("Apis/Lucene/Recipes/luceneQueryTest.json",typeof(LuceneContext).Assembly)
            };
        }

        public LuceneContext()
        {
            this.WithRecipe("luceneQueryTest");
        }

    }
}
