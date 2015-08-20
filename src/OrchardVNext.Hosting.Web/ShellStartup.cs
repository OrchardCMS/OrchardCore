using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Hosting.Mvc;

namespace OrchardVNext.Web {
    public class ShellStartup {
        public void ConfigureServices(IServiceCollection services) {
            services
                .AddOrchardMvc();
        }
    }
}
