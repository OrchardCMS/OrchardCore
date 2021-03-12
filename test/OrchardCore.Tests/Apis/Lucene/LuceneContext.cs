using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
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
            this.WithRecipe("luceneQueryTest");
        }
    }
}
