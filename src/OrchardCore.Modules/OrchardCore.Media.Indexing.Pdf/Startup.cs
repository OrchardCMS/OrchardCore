using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Media.Indexing.Pdf;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMediaFileTextProvider<PdfMediaFileTextProvider>(".pdf");
    }
}
