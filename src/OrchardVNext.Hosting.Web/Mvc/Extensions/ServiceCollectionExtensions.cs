using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.Razor.Compilation;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Hosting.Web.Mvc;
using OrchardVNext.Hosting.Web.Mvc.Razor;
using OrchardVNext.Hosting.Web.Mvc.Routing;

namespace OrchardVNext.Hosting.Mvc {
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