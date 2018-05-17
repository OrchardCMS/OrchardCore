using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules
{
    public class ModularServicesBuilder
    {
        public ModularServicesBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
