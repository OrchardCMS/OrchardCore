using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Media.Indexing.MicrosoftOffice;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMediaFileTextProvider<WordDocumentMediaFileTextProvider>(".docx");
        services.AddMediaFileTextProvider<PresentationDocumentMediaFileTextProvider>(".pptx");
    }
}
