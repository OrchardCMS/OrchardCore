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

            // TheAdmin's stylesheet reads the fonts, the font size, the corners and the density of a scheme.
            options.Features = ThemeSchemeFeatures.All;
            options.Fonts = [.. TheAdminSchemes.Fonts];
            options.DefaultFontFamily = TheAdminSchemes.DefaultFontFamily;
            options.AddTokens<TheAdminSchemeTokens>();

            foreach (var (token, name) in TheAdminSchemes.TokenNames)
            {
                options.TokenNames[token] = name;
            }
        });
    }
}
