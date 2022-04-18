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

            //builder.UseSecurityHeaders(config =>
            //{
            //    config
            //        .AddContentTypeOptions(securityOptions.ContentTypeOptions)
            //        .AddFrameOptions(securityOptions.FrameOptions)
            //        .AddPermissionsPolicy(securityOptions.PermissionsPolicy)
            //        .AddReferrerPolicy(securityOptions.ReferrerPolicy);
            //});

            builder.UseContentTypeOptions();
            builder.UseFrameOptions(new FrameOptionsOptions
            {
                Value = new FrameOptionsValue(securityOptions.FrameOptions)
            });

            if (securityOptions.PermissionsPolicy.Count == 0)
            {
                builder.UsePermissionsPolicy();
            }
            else
            {
                builder.UsePermissionsPolicy(new PermissionsPolicyOptions
                {
                    Values = securityOptions.PermissionsPolicy
                        .Select(p => new PermissionsPolicyValue(p))
                        .ToList(),
                    Origin = new PermissionsPolicyOriginValue(securityOptions.PermissionsPolicyOrigin)
                });
            }

            builder.UseReferrerPolicy(new ReferrerPolicyOptions
            {
                Value = new ReferrerPolicyValue(securityOptions.ReferrerPolicy)
            });

            builder.UseStrictTransportSecurity(securityOptions.StrictTransportSecurity);
        }
    }
}
