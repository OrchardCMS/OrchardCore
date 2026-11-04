using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Modules;

namespace OrchardCore.Contents.Deployment.Download;

[Feature("OrchardCore.Contents.Deployment.Download")]
public sealed class DownloadStartup : StartupBase
{
    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        ContentExportEndpoints.Map(routes);
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddScoped<ContentExportService>();
        services.AddScoped<IContentDisplayDriver, DownloadContentDriver>();
    }
}
