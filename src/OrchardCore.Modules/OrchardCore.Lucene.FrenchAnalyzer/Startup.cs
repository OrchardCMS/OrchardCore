using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Lucene.Services;
using OrchardCore.Modules;

namespace OrchardCore.Lucene.FrenchAnalyzer
{
    [Feature("OrchardCore.Lucene.FrenchAnalyzer")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<LuceneOptions>(o =>
                o.Analyzers.Add(new LuceneAnalyzer("frenchanalyzer",
                    new Analyzers.FrenchAnalyzer(LuceneSettings.DefaultVersion))));
        }
    }
}
