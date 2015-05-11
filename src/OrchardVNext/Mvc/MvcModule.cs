using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Framework.DependencyInjection;

namespace OrchardVNext.Mvc {
    public class MvcModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection.AddMvc();

            serviceCollection.Configure<RazorViewEngineOptions>(options => {
                var expander = new ModuleViewLocationExpander();
                options.ViewLocationExpanders.Add(expander);
            });
        }
    }
}