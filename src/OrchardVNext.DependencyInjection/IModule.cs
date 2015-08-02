using Microsoft.Framework.DependencyInjection;

namespace OrchardVNext {
    public interface IModule {
        void Configure(IServiceCollection serviceCollection);
    }
}