using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
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
            services.AddScoped<IDisplayDriver<ISite>, SecurityHeadersSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddSingleton<SecurityHeadersService>();

            services.AddTransient<IConfigureOptions<SecurityHeadersOptions>, SecurityHeadersOptionsConfiguration>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var securityHeadersOptions = serviceProvider.GetRequiredService<IOptions<SecurityHeadersOptions>>().Value;

            builder.UseSecurityHeaders(config =>
            {
                config.AddReferrerPolicy(securityHeadersOptions.ReferrerPolicy);
            });
        }
    }
}
