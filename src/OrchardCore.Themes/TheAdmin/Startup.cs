using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Themes.TheAdmin.Drivers;

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
        services.AddDisplayDriver<Navbar, ToggleThemeNavbarDisplayDriver>();
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
        services.Configure<TheAdminThemeOptions>(_configuration.GetSection("TheAdminTheme:StyleSettings"));
    }
}
