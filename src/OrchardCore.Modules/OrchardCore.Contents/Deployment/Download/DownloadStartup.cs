using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Modules;

namespace OrchardCore.Contents.Deployment.Download;

[Feature("OrchardCore.Contents.Deployment.Download")]
public sealed class DownloadStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentDisplayDriver, DownloadContentDriver>();
    }
}
