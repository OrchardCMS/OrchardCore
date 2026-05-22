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

        services.PostConfigure<TheAdminThemeOptions>(options =>
        {
            options.WrapperClasses = "row mb-3";
            options.LimitedWidthWrapperClasses = "row";
            options.LimitedWidthClasses = "col-md-6 col-lg-5 col-xxl-4";
            options.StartClasses = "col-lg-2 col-xl-3";
            options.EndClasses = "col-lg-10 col-xl-9";
            options.LabelClasses = "col-form-label text-lg-end col-lg-2 col-xl-3";
            options.OffsetClasses = "offset-lg-2 offset-xl-3";
        });
    }
}
