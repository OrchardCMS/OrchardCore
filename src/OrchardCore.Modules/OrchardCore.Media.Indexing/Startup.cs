using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing;
using OrchardCore.Modules;

namespace OrchardCore.Media.Indexing;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentFieldIndexHandler, MediaFieldIndexHandler>();
    }
}

[Feature("OrchardCore.Media.Indexing.Pdf")]
public class PdfStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMediaFileTextProvider<PdfMediaFileTextProvider>(".pdf");
    }
}

[Feature("OrchardCore.Media.Indexing.Text")]
public class TextStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMediaFileTextProvider<TextMediaFileTextProvider>(".txt");
        services.AddMediaFileTextProvider<TextMediaFileTextProvider>(".md");
    }
}

[Feature("OrchardCore.Media.Indexing.MicrosoftOffice")]
public class MicrosoftOfficeStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMediaFileTextProvider<WordDocumentMediaFileTextProvider>(".docx");
        services.AddMediaFileTextProvider<PresentationDocumentMediaFileTextProvider>(".pptx");
    }
}
