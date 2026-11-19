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
        services.AddThemeSchemes(TheAdminConstants.ThemeId, options =>
        {
            options.AttributeName = TheAdminSchemes.AttributeName;
            options.TokenPrefix = TheAdminSchemes.TokenPrefix;
            options.DefaultSchemeId = TheAdminSchemes.DefaultId;
            options.Defaults = [.. TheAdminSchemes.CreateDefaults()];
            options.AddTokens<TheAdminSchemeTokens>();

            foreach (var (token, name) in TheAdminSchemes.TokenNames)
            {
                options.TokenNames[token] = name;
            }
        });
    }
}
