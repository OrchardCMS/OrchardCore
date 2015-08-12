using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.Razor.Compilation;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.DependencyInjection;
using OrchardVNext.Mvc.Razor;

namespace OrchardVNext.Mvc {
    public class MvcModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection
                .AddMvcCore()
                .AddViews()
                .AddRazorViewEngine();

            serviceCollection.AddScoped<IAssemblyProvider, OrchardMvcAssemblyProvider>();

            serviceCollection.AddSingleton<ICompilationService, DefaultRoslynCompilationService>();

            serviceCollection.Configure<RazorViewEngineOptions>(options => {
                var expander = new ModuleViewLocationExpander();
                options.ViewLocationExpanders.Add(expander);
            });
        }
    }
}