using Microsoft.Extensions.DependencyInjection;

namespace Orchard.DependencyInjection
{
    public interface IModule
    {
        void Configure(IServiceCollection serviceCollection);
    }
}