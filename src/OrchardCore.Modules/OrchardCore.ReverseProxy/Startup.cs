using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.ReverseProxy.Drivers;
using OrchardCore.ReverseProxy.Services;
using OrchardCore.Settings;

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
            services.AddScoped<IDisplayDriver<ISite>, ReverseProxySettingsDisplayDriver>();
            services.AddSingleton<ReverseProxyService>();

            services.TryAddEnumerable(ServiceDescriptor
                .Transient<IConfigureOptions<ForwardedHeadersOptions>, ForwardedHeadersOptionsConfiguration>());
        }
    }
}
