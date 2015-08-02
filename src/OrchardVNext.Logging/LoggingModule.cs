using Microsoft.Framework.DependencyInjection;

namespace OrchardVNext.Logging {
    public class LoggingModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection.AddLogging();
        }
    }
}