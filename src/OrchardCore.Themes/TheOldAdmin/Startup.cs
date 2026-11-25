using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Themes.TheOldAdmin.Drivers;

namespace OrchardCore.Themes.TheOldAdmin;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<Navbar, ToggleThemeNavbarDisplayDriver>();
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
    }
}
