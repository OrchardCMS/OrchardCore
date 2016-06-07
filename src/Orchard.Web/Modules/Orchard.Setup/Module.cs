using Microsoft.Extensions.DependencyInjection;
using Orchard.DependencyInjection;
using Orchard.Routes;

namespace Orchard.Setup
{
    public class Module : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IRouteProvider, Routes>();
        }
    }
}
