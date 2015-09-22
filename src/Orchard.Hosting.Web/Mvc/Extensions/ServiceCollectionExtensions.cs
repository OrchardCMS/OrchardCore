using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.Razor.Compilation;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Orchard.Hosting.Web.Mvc;
using Orchard.Hosting.Web.Mvc.Razor;
using Orchard.Hosting.Web.Mvc.Routing;

namespace Orchard.Hosting.Mvc {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddOrchardMvc([NotNull] this IServiceCollection services) {
            services.AddScoped<IRouteBuilder, DefaultShellRouteBuilder>();

            services
                .AddMvcCore()
                .AddViews()
                .AddRazorViewEngine();

            services.AddScoped<IAssemblyProvider, OrchardMvcAssemblyProvider>();

            services.AddSingleton<ICompilationService, DefaultRoslynCompilationService>();
            
            services.Configure<RazorViewEngineOptions>(options => {
                var expander = new ModuleViewLocationExpander();
                options.ViewLocationExpanders.Add(expander);
            });
            return services;
        }
    }
}