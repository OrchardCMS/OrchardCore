using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.Themes.TheAdmin;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _configuration;

    public Startup(IShellConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddResourceManagementOptionsConfiguration<ResourceManagementOptionsConfiguration>();
        services.Configure<TheAdminThemeOptions>(_configuration.GetSection("TheAdminTheme:StyleSettings"));
    }
}
