using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Upgrade.Controllers;
using OrchardCore.Upgrade.Services;

namespace OrchardCore.Upgrade
{

    [Feature("OrchardCore.Upgrade.UserId")]
    public class UpgradeUserIdStartup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public UpgradeUserIdStartup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, UserIdAdminMenu>();
            services.Configure<MvcOptions>((options) =>
            {
                // Ordered to be called before other filters, but after the admin filter.
                options.Filters.Add(typeof(UserIdUpgradeFilter), -100);
            });
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {

            var userIdControllerName = typeof(UserIdController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "UpgradeUserIdIndex",
                areaName: "OrchardCore.Upgrade",
                pattern: _adminOptions.AdminUrlPrefix + "/Upgrade/UserId",
                defaults: new { controller = userIdControllerName, action = nameof(UserIdController.Index) }
            );
            routes.MapAreaControllerRoute(
                name: "UpgradeUserIdIndexPost",
                areaName: "OrchardCore.Upgrade",
                pattern: _adminOptions.AdminUrlPrefix + "/Upgrade/UserId",
                defaults: new { controller = userIdControllerName, action = nameof(UserIdController.IndexPost) }
            );
        }
    }
}
