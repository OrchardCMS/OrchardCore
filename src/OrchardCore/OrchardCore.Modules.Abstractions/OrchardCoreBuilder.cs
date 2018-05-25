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

    public class OrchardBuilder : OrchardCoreBuilder
    {
        public OrchardBuilder(IServiceCollection services) : base(services) { }
    }
}
