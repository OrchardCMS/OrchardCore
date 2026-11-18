using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Themes.Schemes;
using OrchardCore.Themes.TheAdmin.Drivers;
using OrchardCore.Themes.TheAdmin.Schemes;

namespace OrchardCore.Themes.TheAdmin;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<Navbar, ToggleThemeNavbarDisplayDriver>();
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
        services.AddThemeSchemes<TheAdminSchemeProvider>();
    }
}
