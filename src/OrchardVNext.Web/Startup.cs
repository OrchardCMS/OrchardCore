using Microsoft.AspNet.Builder;
using OrchardVNext.Environment;

namespace OrchardVNext.Web {
    public class Startup {
        public void Configure(IApplicationBuilder app) {
            var host = OrchardStarter.CreateHost(app);
            host.Initialize();
        }
    }
}