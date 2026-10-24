using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Drivers;
using OrchardCore.Users.Models;

namespace OrchardCore.Users;

[Feature(UserConstants.Features.UserSecurity)]
public sealed class UserSecurityStartup : StartupBase
{
    private static readonly string s_securityControllerName = typeof(SecurityController).ControllerName();

    private UserOptions _userOptions;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<UserMenu, SecurityUserMenuDisplayDriver>();
        services.AddSiteDisplayDriver<UserSecuritySettingsDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        _userOptions ??= serviceProvider.GetRequiredService<IOptions<UserOptions>>().Value;

        routes.MapAreaControllerRoute(
            name: "UserSecurity",
            areaName: UserConstants.Features.Users,
            pattern: _userOptions.SecurityPath,
            defaults: new
            {
                controller = s_securityControllerName,
                action = nameof(SecurityController.Index),
            }
        );
    }
}
