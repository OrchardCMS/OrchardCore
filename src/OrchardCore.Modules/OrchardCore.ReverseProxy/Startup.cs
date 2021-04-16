using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.ReverseProxy.Drivers;
using OrchardCore.ReverseProxy.Services;
using OrchardCore.ReverseProxy.Settings;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.ReverseProxy
{
    public class Startup : StartupBase
    {
        // we need this to start before other security related initialization logic
        public override int Order => -1;

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseForwardedHeaders();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddTransient<IDisplayDriver<ISite>, ReverseProxySettingsDisplayDriver>();
            services.AddSingleton<ReverseProxyService>();

            services.TryAddEnumerable(ServiceDescriptor
                .Transient<IConfigureOptions<ForwardedHeadersOptions>, ForwardedHeadersOptionsConfiguration>());
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<ReverseProxySettings>>();
            services.AddTransient<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var S = sp.GetService<IStringLocalizer<DeploymentStartup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<ReverseProxySettings>(S["Reverse Proxy settings"], S["Exports the Reverse Proxy settings."]);
            });
            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<ReverseProxySettings>());
        }
    }
}
