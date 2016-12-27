using System;
using Glimpse;
using Glimpse.Common;
using Glimpse.Common.Initialization;
using Glimpse.Initialization;
using Glimpse.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.Glimpse.Inspectors;

namespace Orchard.Glimpse
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Add(GlimpseServices.GetDefaultServices());

            services.AddTransient<IAssemblyProvider, OrchardAssemblyProvider>();
            services.AddSingleton<IGlimpseContextAccessor, OrchardGlimpseContextAccessor>();
            services.AddTransient<OrchardWebDiagnosticsInspector>();

            var servicesProvider = services
                .BuildServiceProvider();

            var extensionProvider = servicesProvider
                .GetService<IExtensionProvider<IRegisterServices>>();

            var glimpseServiceCollectionBuilder = new GlimpseServiceCollectionBuilder(services);

            foreach (var registration in extensionProvider.Instances)
            {
                registration.RegisterServices(glimpseServiceCollectionBuilder);
            }

            (servicesProvider as IDisposable).Dispose();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseGlimpse();
        }
    }
}
