using Microsoft.Framework.DependencyInjection;

namespace OrchardVNext.DependencyInjection {
    public interface IModule {
        void Configure(IServiceCollection serviceCollection);
    }
}