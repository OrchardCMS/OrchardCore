using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Drivers;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;
using OrchardCore.Settings;

namespace OrchardCore.Security
{
    public class Startup : StartupBase
    {
        public override int Order => -1;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, SecurityPermissions>();
            services.AddScoped<IDisplayDriver<ISite>, SecuritySettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddSingleton<ISecurityService, SecurityService>();

            services.AddTransient<IConfigureOptions<SecuritySettings>, SecuritySettingsConfiguration>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var securityOptions = serviceProvider.GetRequiredService<IOptions<SecuritySettings>>().Value;

            builder.UseSecurityHeaders(config =>
            {
                var permissionsPolicyOptions = new PermissionsPolicyOptions();
                if (securityOptions.PermissionsPolicy.Count > 0)
                {
                    permissionsPolicyOptions.Values = securityOptions.PermissionsPolicy
                        .Select(p => new PermissionsPolicyValue(p))
                        .ToList();
                    permissionsPolicyOptions.Origin = new PermissionsPolicyOriginValue(securityOptions.PermissionsPolicyOrigin);
                }

                config
                    .AddContentTypeOptions()
                    .AddFrameOptions(securityOptions.FrameOptions)
                    .AddPermissionsPolicy(permissionsPolicyOptions)
                    .AddReferrerPolicy(securityOptions.ReferrerPolicy)
                    .AddStrictTransportSecurity(securityOptions.StrictTransportSecurity);
            });
        }
    }
}
