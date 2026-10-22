using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin.QuickNavigation;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;

namespace OrchardCore.Contents.QuickNavigation;

[Feature("OrchardCore.Contents.QuickNavigation")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<QuickNavigationSource, ContentItemNavigationSource>();
        services.AddSiteDisplayDriver<ContentQuickNavigationSettingsDisplayDriver>();
    }
}
