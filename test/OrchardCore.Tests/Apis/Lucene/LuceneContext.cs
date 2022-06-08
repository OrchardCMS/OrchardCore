using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Lucene;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.Lucene
{
    public class LuceneContext : SiteContext
    {
        public static IShellHost ShellHost { get; }

        static LuceneContext()
        {
            ShellHost = Site.Services.GetRequiredService<IShellHost>();
        }

        public LuceneContext()
        {
            this.UseAssemblies(GetType().Assembly);
            this.UseRecipies(new string[] { "Apis/Lucene/Recipes/luceneQueryTest.json" });
            this.WithRecipe("luceneQueryTest");
        }

    }
}
