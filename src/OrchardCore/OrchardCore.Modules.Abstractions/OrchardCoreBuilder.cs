using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules
{
    public class OrchardCoreBuilder
    {
        public OrchardCoreBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
